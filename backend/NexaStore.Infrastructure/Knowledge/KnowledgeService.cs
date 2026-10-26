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
            var normalizedQuery = query.Trim();
            var generated = await embeddings.GenerateAsync(
                normalizedQuery,
                new EmbeddingGenerationOptions { ModelId = options.EmbeddingModel, Dimensions = options.EmbeddingDimensions },
                cancellationToken);

            //Semantic Search
            var queryVector = new Vector(generated.Vector.ToArray());
            //Decides how many candidates to fetch
            var candidateLimit = Math.Clamp(limit * options.CandidateMultiplier, 10, 100);

            var category = InferCategory(normalizedQuery);
            var vectorQuery = dbContext.KnowledgeChunks.AsNoTracking();
            if (category is not null)
            {
                vectorQuery = vectorQuery.Where(chunk => chunk.Document.Category == category);
            }

            // Semantic Search
            var semantic = await vectorQuery
                .AsNoTracking()
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Distance = chunk.Embedding.CosineDistance(queryVector)
                })
                .OrderBy(result => result.Distance)
                .Take(candidateLimit)
                .Select(result => new SearchCandidate(
                    result.Chunk.Id,
                    result.Chunk.Document.Title,
                    result.Chunk.Document.Category,
                    result.Chunk.Document.SourcePath,
                    result.Chunk.PageNumber,
                    result.Chunk.Content,
                    1d - result.Distance,
                    0d))
                .ToArrayAsync(cancellationToken);

            //Keyword Search
            var lexicalText = ExpandQuery(normalizedQuery);
            var lexicalTsQuery = string.Join(" | ", Tokenize(lexicalText));
            var lexicalQuery = dbContext.KnowledgeChunks.AsNoTracking();
            if (category is not null)
            {
                lexicalQuery = lexicalQuery.Where(chunk => chunk.Document.Category == category);
            }

            // PostgreSQL Full-Text Search
            SearchCandidate[] lexical = lexicalTsQuery.Length == 0
                ? []
                : await lexicalQuery
                    .Where(chunk => EF.Functions.ToTsVector("english", chunk.Content)
                        .Matches(EF.Functions.ToTsQuery("english", lexicalTsQuery)))
                    .Select(chunk => new
                    {
                        Chunk = chunk,
                        Score = EF.Functions.ToTsVector("english", chunk.Content)
                            .RankCoverDensity(EF.Functions.ToTsQuery("english", lexicalTsQuery))
                    })
                    .OrderByDescending(candidate => candidate.Score)
                    .Take(candidateLimit)
                    .Select(candidate => new SearchCandidate(
                        candidate.Chunk.Id,
                        candidate.Chunk.Document.Title,
                        candidate.Chunk.Document.Category,
                        candidate.Chunk.Document.SourcePath,
                        candidate.Chunk.PageNumber,
                        candidate.Chunk.Content,
                        0d,
                        candidate.Score))
                    .ToArrayAsync(cancellationToken);

            // Normalize the lexical score
            var maxLexical = lexical.Length == 0 ? 0d : lexical.Max(candidate => candidate.LexicalScore);
            var terms = Tokenize(normalizedQuery);
            
            // Merge semantic + keyword candidates
            var combined = semantic.Concat(lexical)
                .GroupBy(candidate => candidate.ChunkId)
                .Select(group =>
                {
                    var candidate = group.First();
                    var semanticScore = group.Max(item => item.SemanticScore);
                    var lexicalScore = group.Max(item => item.LexicalScore);
                    // Calculate query-term coverage
                    var coverage = CalculateCoverage(candidate.Content, terms);
                    // Ignore weak lexical matches
                    var normalizedLexical = maxLexical <= 0 || coverage < 0.4 ? 0 : lexicalScore / maxLexical;
                    var phraseBoost = candidate.Content.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ? 1d : 0d;
                    var relevance = (semanticScore * 0.65) + (normalizedLexical * 0.25) + (coverage * 0.08) + (phraseBoost * 0.02);
                    var method = semanticScore > 0 && lexicalScore > 0 ? "hybrid" : semanticScore > 0 ? "semantic" : "keyword";
                    return new RankedCandidate(candidate, relevance, method);
                })
                .Where(result => result.Relevance >= options.MinimumRelevance)
                .OrderByDescending(result => result.Relevance)
                .DistinctBy(result => $"{result.Candidate.SourcePath}|{result.Candidate.PageNumber}|{Fingerprint(result.Candidate.Content)}")
                .Take(limit)
                .Select(result => new KnowledgeSearchResult(
                    result.Candidate.ChunkId,
                    result.Candidate.DocumentTitle,
                    result.Candidate.Category,
                    result.Candidate.SourcePath,
                    result.Candidate.PageNumber,
                    result.Candidate.Content,
                    Math.Min(1d, result.Relevance),
                    result.Method,
                    $"[{result.Candidate.DocumentTitle}, p. {result.Candidate.PageNumber}]"))
                .ToArray();

            logger.LogInformation(
                "Hybrid knowledge search returned {Count} results from {SemanticCount} semantic and {LexicalCount} keyword candidates (category {Category})",
                combined.Length,
                semantic.Length,
                lexical.Length,
                category ?? "all");
            return combined;
        }

        private static string ExpandQuery(string query)
        {
            var expansions = new List<string> { query };
            if (query.Contains("LLM", StringComparison.OrdinalIgnoreCase)) expansions.Add("large language model local AI");
            if (query.Contains("VRAM", StringComparison.OrdinalIgnoreCase)) expansions.Add("video memory GPU memory");
            if (query.Contains("return", StringComparison.OrdinalIgnoreCase)) expansions.Add("refund eligibility window");
            if (query.Contains("warranty", StringComparison.OrdinalIgnoreCase)) expansions.Add("coverage repair replacement");
            return string.Join(' ', expansions.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static string? InferCategory(string query)
        {
            var value = query.ToLowerInvariant();
            if (ContainsAny(value, "return", "refund", "warranty", "shipping", "delivery", "cancellation", "price match")) return "policies";
            if (ContainsAny(value, "troubleshoot", "charging", "battery issue", "connectivity", "not working")) return "support";
            if (ContainsAny(value, "membership", "nexaplus", "rewards", "loyalty")) return "customer-programs";
            return null;
        }

        private static bool ContainsAny(string value, params string[] terms) => terms.Any(value.Contains);

        //Converts a question into useful keywords
        private static string[] Tokenize(string value) => value
        .Split([' ', ',', '.', '?', '!', ':', ';', '/', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(term => term.Length > 2 && !StopWords.Contains(term))
        .Select(term => term.ToLowerInvariant())
        .Distinct()
        .ToArray();

        //Checking how much of query appears in chunk
        private static double CalculateCoverage(string content, IReadOnlyCollection<string> terms)
        {
            if (terms.Count == 0) return 0;
            var matches = terms.Count(term => content.Contains(term, StringComparison.OrdinalIgnoreCase));
            return (double)matches / terms.Count;
        }

        private static string Fingerprint(string content) =>
            string.Concat(content.Where(char.IsLetterOrDigit).Take(180)).ToLowerInvariant();

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "from", "with", "what", "which", "does", "are", "this", "that", "how", "can"
    };

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

        private sealed record SearchCandidate(
        Guid ChunkId,
        string DocumentTitle,
        string Category,
        string SourcePath,
        int PageNumber,
        string Content,
        double SemanticScore,
        double LexicalScore);

        private sealed record RankedCandidate(SearchCandidate Candidate, double Relevance, string Method);
    }
}
