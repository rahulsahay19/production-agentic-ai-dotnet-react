namespace NexaStore.Infrastructure.AI
{
    public sealed class SingleAgentOptions
    {
        public const string SectionName = "Agent";
        public string Name { get; init; } = "NexaStore Support & Shopping Agent";
        public int MaximumToolIterations { get; init; } = 5;

        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Name);
            if(MaximumToolIterations is < 1 or > 10)
            {
                throw new InvalidOperationException("Agent:MaximumToolIterations must be between 1 and 10");
            }
        }
    }
}
