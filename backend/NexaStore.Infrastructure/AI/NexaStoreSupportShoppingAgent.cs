using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;
using System.Text;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaStoreSupportShoppingAgent(
      IChatClient chatClient,
      AiOptions options,
      SingleAgentOptions agentOptions,
      IJevDecisionService jevDecisionService,
      JevRunContext jevContext,
      IAssistantToolProvider toolProvider,
      AiToolActivityTracker activityTracker,
      ILogger<NexaStoreSupportShoppingAgent> logger) : INexaAssistantService
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = AssistantResponseStyles.Balanced,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var decision = jevDecisionService.Decide(messages);
            jevContext.Begin(decision);
            yield return AssistantStreamUpdate.GovernanceDecision(decision);

            if (!decision.Allowed)
            {
                const string blockedResponse =
                    "I can help you evaluate products and check availability, but I cannot modify carts, place or cancel orders, process refunds, or change account data. No action was taken.";
                yield return AssistantStreamUpdate.Delta(blockedResponse);
                yield return AssistantStreamUpdate.GovernanceVerification(jevContext.Verify(blockedResponse));
                yield break;
            }

            var chatMessages = new List<ChatMessage>(messages.Count + 1)
        {
            new(ChatRole.System, AssistantPromptFactory.Create(responseStyle, decision))
        };

            chatMessages.AddRange(messages.Select(message => new ChatMessage(
                string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.Assistant
                    : ChatRole.User,
                message.Content.Trim())));

            var runId = Guid.NewGuid();
            logger.LogInformation(
                "Starting agent run {AgentRunId} for {AgentName} with {Provider} and {MessageCount} conversation messages",
                runId,
                agentOptions.Name,
                options.Provider,
                messages.Count);

            var availableTools = await toolProvider.GetToolsAsync(cancellationToken);
            var chatOptions = new ChatOptions
            {
                MaxOutputTokens = options.MaxOutputTokens,
                Tools = availableTools
            };

            if (string.Equals(options.Provider, AiProviders.Ollama, StringComparison.OrdinalIgnoreCase))
            {
                chatOptions.Temperature = options.Temperature;
            }
            else
            {
                chatOptions.Reasoning = new ReasoningOptions
                {
                    Effort = ReasoningEffort.Medium
                };
            }

            logger.LogDebug(
                "AI request options selected for {Provider}: temperature configured = {HasTemperature}, reasoning effort = {ReasoningEffort}",
                options.Provider,
                chatOptions.Temperature.HasValue,
                chatOptions.Reasoning?.Effort);

            var response = new StringBuilder();
            await foreach (var update in chatClient.GetStreamingResponseAsync(
                chatMessages,
                chatOptions,
                cancellationToken: cancellationToken))
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

            foreach (var activity in activityTracker.Drain())
            {
                yield return activity;
            }

            var verification = jevContext.Verify(response.ToString());
            if (!verification.Passed)
            {
                logger.LogWarning(
                    "JEV verification failed for agent run {AgentRunId}. Missing evidence: {MissingEvidence}",
                    runId,
                    string.Join(", ", verification.MissingEvidence));
                yield return AssistantStreamUpdate.Delta(
                    "\n\n> Evidence verification could not confirm every required source. Treat this response as unverified.");
            }
            yield return AssistantStreamUpdate.GovernanceVerification(verification);

            logger.LogInformation(
                "Completed agent run {AgentRunId} for {AgentName}",
                runId,
                agentOptions.Name);
        }
    }
}
