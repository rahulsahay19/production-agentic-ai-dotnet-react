namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeDocument
    {
        public Guid Id { get; set; }
        public required string SourcePath { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
        public required string ContentHash { get; set; }
        public required string EmbeddingModel { get; set; }
        public DateTimeOffset IngestedAt { get; set; }
        public ICollection<KnowledgeChunk> Chunks { get; set; } = [];
    }
}
