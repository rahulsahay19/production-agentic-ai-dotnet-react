namespace NexaStore.Application.AI
{
    public sealed record AssistantMessageDto(string Role, string Content);
    public sealed record AssistantChatRequest(IReadOnlyList<AssistantMessageDto> Messages,
        string ResponseStyle = AssistantResponseStyles.Balanced);

    public static class AssistantResponseStyles
    {
        public const string Concise = "concise";
        public const string Balanced = "balanced";
        public const string Detailed = "detailed";

        public static readonly IReadOnlyList<string> All = [Concise, Balanced, Detailed];
    }
    public sealed record AssistantStatusDto(
        bool Enabled,
        string Provider,
        string Model,
        IReadOnlyList<string> Capabilities,
        IReadOnlyList<string> ResponseStyles);
    public sealed record AgentProfileDto(
        string Name,
        string Role,
        string Orchestration,
        int MaximumToolIterations,
        IReadOnlyList<string> Goals,
        IReadOnlyList<string> Boundaries
        );

    public sealed record JevDecisionDto(
        string Route,
        string Risk,
        string EvidenceRequirement,
        bool Allowed,
        string ReasonCode,
        IReadOnlyList<string> RequiredTools);

    public sealed record JevVerificationDto(
        bool Passed,
        string Status,
        IReadOnlyList<string> EvidenceUsed,
        IReadOnlyList<string> MissingEvidence);

    public sealed record JevProfileDto(
        bool Enabled,
        string Strategy,
        IReadOnlyList<string> Routes,
        IReadOnlyList<string> EvidencePolicies,
        IReadOnlyList<string> Enforcement);

    public sealed record AssistantStreamUpdate(
        string Type,
        string? Content = null,
        string? ToolName = null,
        string? ToolLabel = null,
        JevDecisionDto? Decision = null,
        JevVerificationDto? Verification = null)
    {
        public static AssistantStreamUpdate Delta(string content) => new("delta", Content: content);
        public static AssistantStreamUpdate Tool(string name, string label) => new("tool", ToolName:name, ToolLabel: label);
        public static AssistantStreamUpdate GovernanceDecision(JevDecisionDto decision) =>
            new("governance", Decision:decision);
        public static AssistantStreamUpdate GovernanceVerification(JevVerificationDto verification) =>
            new("verification", Verification:verification);

    }

    public interface INexaAssistantService
    {
        IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = AssistantResponseStyles.Balanced,
            CancellationToken cancellationToken = default);
    }
}
