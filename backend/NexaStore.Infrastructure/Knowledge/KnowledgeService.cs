using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.Knowledge;
using NexaStore.Infrastructure.Persistence;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Security.Cryptography;
using UglyToad.PdfPig;

namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeService(
        NexaStoreDbContext dbContext,
        IEmbeddingGenerator<string, Embedding<float>> embeddings,
        KnowledgeOptions options,
        ILogger<KnowledgeService> logger) : IKnowledgeService
    {
        public async Task<KnowledgeStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
        => new(
                await dbContext.KnowledgeDocuments.CountAsync(cancellationToken),
                await dbContext.KnowledgeChunks.CountAsync(cancellationToken),
                await dbContext.KnowledgeDocuments.MaxAsync(document => (DateTimeOffset?)document.IngestedAt, cancellationToken));


        public async Task<KnowledgeIngestionResult> IngestAsync(CancellationToken cancellationToken = default)
        {
            var root = Path.GetFullPath(options.RootPath);
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException($"Knowledge-base directory was not found: {root}");
            }

            var files = Directory.EnumerateFiles(root, "*.pdf", SearchOption.AllDirectories)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var relativePaths = files.Select(path => NormalizePath(Path.GetRelativePath(root, path))).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingDocuments = await dbContext.KnowledgeDocuments
                .Include(document => document.Chunks)
                .ToDictionaryAsync(document => document.SourcePath, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var ingested = 0;
            var skipped = 0;
            var removed = 0;
            var createdChunks = 0;

            foreach (var stale in existingDocuments.Values.Where(document => !relativePaths.Contains(document.SourcePath)))
            {
                dbContext.KnowledgeDocuments.Remove(stale);
                removed++;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourcePath = NormalizePath(Path.GetRelativePath(root, file));
                var hash = await ComputeHashAsync(file, cancellationToken);
                if (existingDocuments.TryGetValue(sourcePath, out var existing)
                    && existing.ContentHash == hash
                    && existing.EmbeddingModel == options.EmbeddingModel)
                {
                    skipped++;
                    continue;
                }

                if (existing is not null)
                {
                    dbContext.KnowledgeDocuments.Remove(existing);
                }

                var pages = ExtractPages(file);
                var chunks = pages.SelectMany(page => ChunkPage(page.PageNumber, page.Text)).ToArray();
                if (chunks.Length == 0)
                {
                    logger.LogWarning("Skipped empty PDF {SourcePath}", sourcePath);
                    skipped++;
                    continue;
                }

                var vectors = new List<Embedding<float>>(chunks.Length);
                foreach (var batch in chunks.Chunk(options.EmbeddingBatchSize))
                {
                    var generated = await embeddings.GenerateAsync(
                        batch.Select(chunk => chunk.Content),
                        new EmbeddingGenerationOptions
                        {
                            ModelId = options.EmbeddingModel,
                            Dimensions = options.EmbeddingDimensions
                        },
                        cancellationToken);
                    vectors.AddRange(generated);
                }

                if (vectors.Count != chunks.Length)
                {
                    throw new InvalidOperationException($"Embedding count mismatch for {sourcePath}.");
                }

                var document = new KnowledgeDocument
                {
                    Id = Guid.NewGuid(),
                    SourcePath = sourcePath,
                    Title = Path.GetFileNameWithoutExtension(file).Replace('-', ' '),
                    Category = GetCategory(sourcePath),
                    ContentHash = hash,
                    EmbeddingModel = options.EmbeddingModel,
                    IngestedAt = DateTimeOffset.UtcNow
                };

                for (var index = 0; index < chunks.Length; index++)
                {
                    document.Chunks.Add(new KnowledgeChunk
                    {
                        Id = Guid.NewGuid(),
                        ChunkIndex = index,
                        PageNumber = chunks[index].PageNumber,
                        Content = chunks[index].Content,
                        Embedding = new Vector(vectors[index].Vector.ToArray())
                    });
                }

                dbContext.KnowledgeDocuments.Add(document);
                await dbContext.SaveChangesAsync(cancellationToken);
                ingested++;
                createdChunks += chunks.Length;
                logger.LogInformation("Ingested {SourcePath} into {ChunkCount} chunks", sourcePath, chunks.Length);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return new KnowledgeIngestionResult(files.Length, ingested, skipped, removed, createdChunks);
        }
        public async Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(
            string query,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            limit = Math.Clamp(limit, 1, 20);
            var generated = await embeddings.GenerateAsync(
                query.Trim(),
                new EmbeddingGenerationOptions { ModelId = options.EmbeddingModel, Dimensions = options.EmbeddingDimensions },
                cancellationToken);
            var queryVector = new Vector(generated.Vector.ToArray());

            return await dbContext.KnowledgeChunks
                .AsNoTracking()
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Distance = chunk.Embedding.CosineDistance(queryVector)
                })
                .OrderBy(result => result.Distance)
                .Take(limit)
                .Select(result => new KnowledgeSearchResult(
                    result.Chunk.Id,
                    result.Chunk.Document.Title,
                    result.Chunk.Document.Category,
                    result.Chunk.Document.SourcePath,
                    result.Chunk.PageNumber,
                    result.Chunk.Content,
                    1d - result.Distance))
                .ToArrayAsync(cancellationToken);
        }

        private IEnumerable<PageChunk> ChunkPage(int pageNumber, string text)
        {
            var normalized = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            if (normalized.Length == 0) yield break;

            var start = 0;
            while (start < normalized.Length)
            {
                var length = Math.Min(options.ChunkSize, normalized.Length - start);
                if (start + length < normalized.Length)
                {
                    var boundary = normalized.LastIndexOf(' ', start + length - 1, length);
                    if (boundary > start + options.ChunkSize / 2) length = boundary - start;
                }

                yield return new PageChunk(pageNumber, normalized.Substring(start, length).Trim());
                if (start + length >= normalized.Length) break;
                start += Math.Max(1, length - options.ChunkOverlap);
            }
        }

        private static IReadOnlyCollection<PageChunk> ExtractPages(string path)
        {
            using var document = PdfDocument.Open(path);
            return document.GetPages().Select(page => new PageChunk(page.Number, page.Text)).ToArray();
        }

        private static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(path);
            var hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return Convert.ToHexString(hash);
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/');

        private static string GetCategory(string sourcePath)
        {
            var folder = sourcePath.Split('/')[0];
            var separator = folder.IndexOf('-');
            return separator >= 0 ? folder[(separator + 1)..] : folder;
        }

        private sealed record PageChunk(int PageNumber, string Content)
        {
            public string Text => Content;
        }
    }
}
