namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeOptions
    {
        public const string SectionName = "Knowledge";

        public string RootPath { get; init; } = "knowledge-base";
        public string EmbeddingModel { get; init; } = "text-embedding-3-small";
        public int EmbeddingDimensions { get; init; } = 1536;
        public int ChunkSize { get; init; } = 2400;
        public int ChunkOverlap { get; init; } = 300;
        public int EmbeddingBatchSize { get; init; } = 32;
        public int DefaultSearchLimit { get; init; } = 5;
        public int CandidateMultiplier { get; init; } = 4;
        public double MinimumRelevance { get; init; } = 0.35;

        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(RootPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(EmbeddingModel);
            if (EmbeddingDimensions is < 1 or > 4096) throw new InvalidOperationException("Knowledge:EmbeddingDimensions must be between 1 and 4096.");
            if (ChunkSize is < 500 or > 8000) throw new InvalidOperationException("Knowledge:ChunkSize must be between 500 and 8000 characters.");
            if (ChunkOverlap is < 0 || ChunkOverlap >= ChunkSize) throw new InvalidOperationException("Knowledge:ChunkOverlap must be non-negative and smaller than ChunkSize.");
            if (EmbeddingBatchSize is < 1 or > 256) throw new InvalidOperationException("Knowledge:EmbeddingBatchSize must be between 1 and 256.");
            if (DefaultSearchLimit is < 1 or > 20) throw new InvalidOperationException("Knowledge:DefaultSearchLimit must be between 1 and 20.");
            if (CandidateMultiplier is < 2 or > 10) throw new InvalidOperationException("Knowledge:CandidateMultiplier must be between 2 and 10.");
            if (MinimumRelevance is < 0 or > 1) throw new InvalidOperationException("Knowledge:MinimumRelevance must be between 0 and 1.");
        }
    }
}
