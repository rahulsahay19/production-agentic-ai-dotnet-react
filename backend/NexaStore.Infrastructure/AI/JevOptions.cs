namespace NexaStore.Infrastructure.AI
{
    public sealed class JevOptions
    {
        public const string SectionName = "Jev";
        public bool Enabled { get; init; } = true;
        public bool BlockUnsupportedMutations { get; init; } = true;
        public bool RequireDocumentCitations { get; init; } = true;

        public void Validate()
        {

        }
    }
}
