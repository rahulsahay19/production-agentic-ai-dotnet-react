using NexaStore.Application.AI;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaStoreSupportShoppingAgent(
    IAgentHarness harness,
    SingleAgentOptions options) : INexaAssistantService
    {
        private readonly AgentDefinition _definition =
            AssistantPromptFactory.CreateSupportShoppingDefinition(options);

        public IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = AssistantResponseStyles.Balanced,
            CancellationToken cancellationToken = default) =>
            harness.RunAsync(
                _definition,
                new AgentRunRequest(messages, responseStyle),
                cancellationToken);
    }
}
