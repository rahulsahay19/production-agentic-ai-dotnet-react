namespace NexaStore.Infrastructure.AI
{
    public sealed class AgentHarnessOptions
    {
        public const string SectionName = "AgentHarness";
        public bool Enabled { get; init; } = true;
        public int MaximumRunDurationSeconds { get; init; } = 120;
        public int MaximumConversationMessages { get; init; } = 20;
        public int MaximumAvailableTools { get; init; } = 20;

        public void Validate()
        {
            if(MaximumRunDurationSeconds is < 5 or > 600)
            {
                throw new InvalidOperationException("AgentHarness:MaximumRunDurationSeconds must be between 5 and 600.");
            }
            if (MaximumConversationMessages is < 1 or > 100)
            {
                throw new InvalidOperationException("AgentHarness:MaximumConversationMessages must be between 1 and 100.");
            }
            if (MaximumAvailableTools is < 1 or > 100)
            {
                throw new InvalidOperationException("AgentHarness:MaximumAvailableTools must be between 1 and 100.");
            }
        }
    }
}
