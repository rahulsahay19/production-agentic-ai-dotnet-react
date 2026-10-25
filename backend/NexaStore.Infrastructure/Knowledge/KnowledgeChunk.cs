using Pgvector;

namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeChunk
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public KnowledgeDocument Document { get; set; } = null!;
        public int ChunkIndex { get; set; }
        public int PageNumber { get; set; }
        public required string Content { get; set; }
        public required Vector Embedding { get; set; }
    }
}
