using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaAssistantService(
     IChatClient chatClient,
     AiOptions options,
     IAssistantToolProvider toolProvider,
     AiToolActivityTracker activityTracker,
     ILogger<NexaAssistantService> logger) : INexaAssistantService
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = AssistantResponseStyles.Balanced,
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

            logger.LogInformation(
                "Starting {Provider} AI response for a conversation with {MessageCount} messages",
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
                    yield return AssistantStreamUpdate.Delta(update.Text);
                }
            }

            foreach (var activity in activityTracker.Drain())
            {
                yield return activity;
            }
        }
    }
}