using NexaStore.Application.AI;

namespace NexaStore.Infrastructure.AI
{
    public sealed record AgentDefinition(
    string Name,
    string Role,
    string Instructions,
    IReadOnlySet<string> AllowedTools);

    public sealed record AgentRunRequest(
        IReadOnlyList<AssistantMessageDto> Messages,
        string ResponseStyle);

    public sealed record AgentRunContext(
        Guid RunId,
        AgentDefinition Agent,
        DateTimeOffset StartedAt,
        JevDecisionDto Decision,
        IReadOnlyList<string> AvailableTools);

    public sealed record AgentRunEvent(
        Guid RunId,
        string AgentName,
        string Type,
        DateTimeOffset OccurredAt,
        IReadOnlyDictionary<string, object?> Data);

    public static class AgentRunEventTypes
    {
        public const string Started = "run-started";
        public const string ContextCreated = "context-created";
        public const string PolicyBlocked = "policy-blocked";
        public const string EvidenceUnavailable = "evidence-unavailable";
        public const string VerificationCompleted = "verification-completed";
        public const string Completed = "run-completed";
    }

    public interface IAgentRunObserver
    {
        ValueTask OnEventAsync(
            AgentRunEvent runEvent,
            CancellationToken cancellationToken = default);
    }

    public interface IAgentHarness
    {
        IAsyncEnumerable<AssistantStreamUpdate> RunAsync(
            AgentDefinition agent,
            AgentRunRequest request,
            CancellationToken cancellationToken = default);
    }
}
