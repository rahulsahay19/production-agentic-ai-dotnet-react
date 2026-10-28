using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaStoreSupportShoppingAgent(
        IChatClient chatClient,
        AiOptions options,
        SingleAgentOptions agentOptions,
        IAssistantToolProvider toolProvider,
        AiToolActivityTracker activityTracker,
        ILogger<NexaStoreSupportShoppingAgent> logger) : INexaAssistantService
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = "balanced",
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatMessages = new List<ChatMessage>(messages.Count + 1)
            {
                new(ChatRole.System, AssistantPromptFactory.Create(responseStyle))
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

            if(string.Equals(options.Provider, AiProviders.Ollama, StringComparison.OrdinalIgnoreCase))
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
                "AI request options selected for {Provider}: temperature configured = {HasTemperature}, reasoniong effort = {ReasoningEffort}",
                options.Provider,
                chatOptions.Temperature.HasValue,
                chatOptions.Reasoning?.Effort);

            await foreach (var update in chatClient.GetStreamingResponseAsync(
                chatMessages,
                chatOptions,
                cancellationToken: cancellationToken))
            {
                foreach(var activity in activityTracker.Drain())
                {
                    yield return activity;
                }
                if(!string.IsNullOrEmpty(update.Text))
                {
                    yield return AssistantStreamUpdate.Delta(update.Text);
                }
            }
            foreach (var activity in activityTracker.Drain())
            {
                yield return activity;
            }
            logger.LogInformation(
                "Completed agent run {AgentRunId} for {AgentName}",
                runId,
                agentOptions.Name);
        }
    }
}
