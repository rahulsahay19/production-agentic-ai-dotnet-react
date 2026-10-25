namespace NexaStore.Application.Knowledge
{
    public sealed record KnowledgeIngestionResult
    (
        int DiscoveredDocuments,
        int IngestedDocuments,
        int SkippedDocuments,
        int RemovedDocuments,
        int CreatedChunks
    );

    public sealed record KnowledgeStatusDto(
        int Documents,
        int Chunks,
        DateTimeOffset? LastIngestedAt);

    public sealed record KnowledgeSearchResult(
        Guid ChunkId,
        string DocumentTitle,
        string Category,
        string SourcePath,
        int PageNumber,
        string Content,
        double Relevance);

    public interface IKnowledgeService
    {
        Task<KnowledgeIngestionResult> IngestAsync(CancellationToken cancellationToken = default);
        Task<KnowledgeStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(
            string query,
            int limit = 5,
            CancellationToken cancellationToken = default
            );
    }
}
