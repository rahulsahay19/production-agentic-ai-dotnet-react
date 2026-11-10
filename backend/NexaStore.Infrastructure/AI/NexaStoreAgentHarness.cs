using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;
using System.Text;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaStoreAgentHarness(
     IChatClient chatClient,
     AiOptions aiOptions,
     SingleAgentOptions agentOptions,
     AgentHarnessOptions harnessOptions,
     IJevDecisionService jevDecisionService,
     JevRunContext jevContext,
     IAssistantToolProvider toolProvider,
     AiToolActivityTracker activityTracker,
     IEnumerable<IAgentRunObserver> observers,
     ILogger<NexaStoreAgentHarness> logger) : IAgentHarness
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> RunAsync(
            AgentDefinition agent,
            AgentRunRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ValidateRequest(agent, request);

            var runId = Guid.NewGuid();
            var startedAt = DateTimeOffset.UtcNow;
            using var logScope = logger.BeginScope(new Dictionary<string, object>
            {
                ["AgentRunId"] = runId,
                ["AgentName"] = agent.Name
            });
            using var runCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (harnessOptions.Enabled)
            {
                runCancellation.CancelAfter(TimeSpan.FromSeconds(harnessOptions.MaximumRunDurationSeconds));
            }
            var runToken = runCancellation.Token;

            //Judgement Part
            var decision = jevDecisionService.Decide(request.Messages);
            jevContext.Begin(decision);
            await PublishAsync(
                runId,
                agent,
                AgentRunEventTypes.Started,
                new Dictionary<string, object?>
                {
                    ["route"] = decision.Route,
                    ["risk"] = decision.Risk,
                    ["messageCount"] = request.Messages.Count
                },
                runToken);
            yield return AssistantStreamUpdate.GovernanceDecision(decision);

            logger.LogInformation(
                "Starting harness run for {AgentName}. Route {JevRoute}, risk {JevRisk}, message count {MessageCount}",
                agent.Name,
                decision.Route,
                decision.Risk,
                request.Messages.Count);

            if (!decision.Allowed)
            {
                const string blockedResponse =
                    "I can help you evaluate products and check availability, but I cannot modify carts, place or cancel orders, process refunds, or change account data. No action was taken.";
                yield return AssistantStreamUpdate.Delta(blockedResponse);
                var blockedVerification = jevContext.Verify(blockedResponse);
                yield return AssistantStreamUpdate.GovernanceVerification(blockedVerification);
                await PublishAsync(
                    runId,
                    agent,
                    AgentRunEventTypes.PolicyBlocked,
                    new Dictionary<string, object?> { ["reasonCode"] = decision.ReasonCode },
                    runToken);
                await PublishAsync(
                    runId,
                    agent,
                    AgentRunEventTypes.Completed,
                    new Dictionary<string, object?> { ["status"] = blockedVerification.Status },
                    runToken);
                logger.LogInformation("Harness run was blocked by JEV policy {ReasonCode}", decision.ReasonCode);
                yield break;
            }

            var discoveredTools = await toolProvider.GetToolsAsync(runToken);
            var availableTools = discoveredTools
                .Where(tool => agent.AllowedTools.Contains(tool.Name))
                .Take(harnessOptions.MaximumAvailableTools)
                .ToArray();
            var deniedTools = discoveredTools
                .Where(tool => !agent.AllowedTools.Contains(tool.Name))
                .Select(tool => tool.Name)
                .Order(StringComparer.Ordinal)
                .ToArray();

            if (deniedTools.Length > 0)
            {
                logger.LogWarning(
                    "Harness withheld tools that are not allowed for {AgentName}: {DeniedTools}",
                    agent.Name,
                    string.Join(", ", deniedTools));
            }

            var context = new AgentRunContext(
                runId,
                agent,
                startedAt,
                decision,
                availableTools.Select(tool => tool.Name).Order(StringComparer.Ordinal).ToArray());
            LogRunContext(context);
            await PublishAsync(
                runId,
                agent,
                AgentRunEventTypes.ContextCreated,
                new Dictionary<string, object?> { ["availableTools"] = context.AvailableTools },
                runToken);

            var unavailableEvidence = decision.RequiredTools
                .Except(context.AvailableTools, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (unavailableEvidence.Length > 0)
            {
                const string unavailableResponse =
                    "I cannot complete this request because the trusted evidence source required for it is currently unavailable. No unsupported answer was generated.";
                yield return AssistantStreamUpdate.Delta(unavailableResponse);
                var unavailableVerification = jevContext.Verify(unavailableResponse);
                yield return AssistantStreamUpdate.GovernanceVerification(unavailableVerification);
                await PublishAsync(
                    runId,
                    agent,
                    AgentRunEventTypes.EvidenceUnavailable,
                    new Dictionary<string, object?> { ["missingTools"] = unavailableEvidence },
                    runToken);
                await PublishAsync(
                    runId,
                    agent,
                    AgentRunEventTypes.Completed,
                    new Dictionary<string, object?> { ["status"] = unavailableVerification.Status },
                    runToken);
                logger.LogWarning(
                    "Harness stopped before model invocation because required evidence tools were unavailable: {MissingTools}",
                    string.Join(", ", unavailableEvidence));
                yield break;
            }

            var chatMessages = CreateMessages(agent, request, decision);
            var chatOptions = CreateChatOptions(availableTools);
            var response = new StringBuilder();

            await foreach (var update in chatClient.GetStreamingResponseAsync(
                chatMessages,
                chatOptions,
                cancellationToken: runToken))
            {
                foreach (var activity in activityTracker.Drain())
                {
                    yield return activity;
                }

                if (!string.IsNullOrEmpty(update.Text))
                {
                    response.Append(update.Text);
                    yield return AssistantStreamUpdate.Delta(update.Text);
                }
            }
            // Tool activities are streamed
            foreach (var activity in activityTracker.Drain())
            {
                yield return activity;
            }

            var verification = jevContext.Verify(response.ToString());
            if (!verification.Passed)
            {
                logger.LogWarning(
                    "Harness evidence verification failed. Missing evidence: {MissingEvidence}",
                    string.Join(", ", verification.MissingEvidence));
                yield return AssistantStreamUpdate.Delta(
                    "\n\n> Evidence verification could not confirm every required source. Treat this response as unverified.");
            }

            yield return AssistantStreamUpdate.GovernanceVerification(verification);
            await PublishAsync(
                runId,
                agent,
                AgentRunEventTypes.VerificationCompleted,
                new Dictionary<string, object?>
                {
                    ["passed"] = verification.Passed,
                    ["status"] = verification.Status,
                    ["evidenceUsed"] = verification.EvidenceUsed
                },
                runToken);
            await PublishAsync(
                runId,
                agent,
                AgentRunEventTypes.Completed,
                new Dictionary<string, object?>
                {
                    ["status"] = verification.Status,
                    ["elapsedMilliseconds"] = (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds
                },
                runToken);
            logger.LogInformation(
                "Completed harness run for {AgentName} in {ElapsedMilliseconds} ms with verification status {VerificationStatus}",
                agent.Name,
                (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
                verification.Status);
        }

        private void ValidateRequest(AgentDefinition agent, AgentRunRequest request)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(agent.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(agent.Instructions);

            if (request.Messages.Count == 0)
            {
                throw new InvalidOperationException("An agent run requires at least one conversation message.");
            }

            if (harnessOptions.Enabled
                && request.Messages.Count > harnessOptions.MaximumConversationMessages)
            {
                throw new InvalidOperationException(
                    $"An agent run cannot exceed {harnessOptions.MaximumConversationMessages} conversation messages.");
            }

            if (!AssistantResponseStyles.All.Contains(request.ResponseStyle, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported response style '{request.ResponseStyle}'.");
            }
        }

        private static List<ChatMessage> CreateMessages(
            AgentDefinition agent,
            AgentRunRequest request,
            JevDecisionDto decision)
        {
            var messages = new List<ChatMessage>(request.Messages.Count + 1)
        {
            new(ChatRole.System, AssistantPromptFactory.Create(agent, request.ResponseStyle, decision))
        };
            messages.AddRange(request.Messages.Select(message => new ChatMessage(
                string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.Assistant
                    : ChatRole.User,
                message.Content.Trim())));
            return messages;
        }

        private ChatOptions CreateChatOptions(IReadOnlyList<AITool> tools)
        {
            var options = new ChatOptions
            {
                MaxOutputTokens = aiOptions.MaxOutputTokens,
                Tools = tools.ToList()
            };

            if (string.Equals(aiOptions.Provider, AiProviders.Ollama, StringComparison.OrdinalIgnoreCase))
            {
                options.Temperature = aiOptions.Temperature;
            }
            else
            {
                options.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.Medium };
            }

            logger.LogDebug(
                "Harness selected request options for {Provider}: maximum tool iterations {MaximumToolIterations}, tool count {ToolCount}",
                aiOptions.Provider,
                agentOptions.MaximumToolIterations,
                tools.Count);
            return options;
        }

        private void LogRunContext(AgentRunContext context) => logger.LogInformation(
            "Harness context created with tools {AvailableTools} and evidence policy {EvidencePolicy}",
            string.Join(", ", context.AvailableTools),
            context.Decision.EvidenceRequirement);

        private async ValueTask PublishAsync(
            Guid runId,
            AgentDefinition agent,
            string type,
            IReadOnlyDictionary<string, object?> data,
            CancellationToken cancellationToken)
        {
            var runEvent = new AgentRunEvent(
                runId,
                agent.Name,
                type,
                DateTimeOffset.UtcNow,
                data);
            foreach (var observer in observers)
            {
                await observer.OnEventAsync(runEvent, cancellationToken);
            }
        }
    }

}
