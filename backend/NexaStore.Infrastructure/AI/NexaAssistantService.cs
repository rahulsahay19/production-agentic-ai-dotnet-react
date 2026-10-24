using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaAssistantService(
    IChatClient chatClient,
    AiOptions options,
    NexaStoreAiTools tools,
    AiToolActivityTracker activityTracker,
    ILogger<NexaAssistantService> logger) : INexaAssistantService
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

            logger.LogInformation(
                "Starting {provider} AI response for a conversation with {MessageCount} messages",
                options.Provider,
                messages.Count);
            await foreach (var update in chatClient.GetStreamingResponseAsync(
                chatMessages,
                new ChatOptions
                {
                    Temperature = options.Temperature,
                    MaxOutputTokens = options.MaxOutputTokens,
                    Tools = tools.CreateAll()
                },
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

