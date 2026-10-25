# Module 06: OpenAI RAG with PostgreSQL and pgvector

This module adds retrieval-augmented generation without changing the provider-neutral chat boundary. OpenAI creates document and query embeddings; PostgreSQL with pgvector owns the vectors; the assistant reaches retrieval only through an allowlisted read-only tool.

## Flow

```text
knowledge-base/**/*.pdf
    -> PdfPig page extraction
    -> whitespace normalization and overlapping page-aware chunks
    -> OpenAI text-embedding-3-small (1536 dimensions)
    -> PostgreSQL knowledge_documents + knowledge_chunks
    -> pgvector HNSW cosine search
    -> search_knowledge_base tool
    -> cited assistant answer
```

## Corpus layout

The relative folder determines the stored category. PDFs live under:

```text
knowledge-base/
  01-products/
  02-buying-guides/
  03-policies/
  04-support/
  05-customer-programs/
  06-catalogues/
```

The folder is mounted read-only at `/app/knowledge-base` in the API container. Generated chunks and vectors live only in PostgreSQL.

## Configuration

Copy `.env.example` to `.env` and provide `OPENAI_API_KEY`. The main settings are:

```dotenv
OPENAI_EMBEDDING_MODEL=text-embedding-3-small
OPENAI_EMBEDDING_DIMENSIONS=1536
```

Chunk size, overlap, batch size, and result limits are configurable in `backend/NexaStore.Api/appsettings.json`. The database image includes the pgvector extension, and the EF Core migration creates `vector(1536)` storage plus an HNSW `vector_cosine_ops` index.

The embedding model and dimension are stored on each document. If either changes, rebuild the corpus so stored and query vectors remain compatible.

## Run and ingest

### Docker Compose

```powershell
docker compose up -d --build
Invoke-RestMethod -Method Post http://localhost:5175/api/knowledge/ingest
```

Docker Compose applies pending EF Core migrations automatically because the API
container sets `Database__ApplyMigrationsOnStartup=true`. The
`AddKnowledgeBaseVectors` migration enables the PostgreSQL `vector` extension,
creates `knowledge_documents` and `knowledge_chunks`, and adds the HNSW cosine
index. Startup applies migration files already present in the image; it never
generates new migration source files.

### Migration lifecycle

Creating and applying migrations are intentionally separate operations:

```text
dotnet ef migrations add  -> creates versioned migration source files
dotnet ef database update -> applies those files to a running database
docker compose up         -> API automatically applies existing pending migrations
```

After intentionally changing the EF model, create the migration once in the
authoritative development checkout:

```powershell
dotnet ef migrations add AddKnowledgeBaseVectors --project backend/NexaStore.Infrastructure --startup-project backend/NexaStore.Api
```

Commit or copy both timestamped migration files and
`NexaStoreDbContextModelSnapshot.cs` together. Every checkout and deployment
must use the same migration ID. Do not independently generate the same schema
change in a second checkout.

### Run the migration manually

When running the API outside Docker, start PostgreSQL and apply all pending
migrations from the repository root. The .NET CLI does not automatically read
`.env`, so supply the database credentials explicitly:

```powershell
docker compose up -d postgres
dotnet ef database update --project backend/NexaStore.Infrastructure --startup-project backend/NexaStore.Api --connection "Host=localhost;Port=5432;Database=nexastore;Username=nexastore;Password=<POSTGRES_PASSWORD>"
```

The target PostgreSQL server must support pgvector. The Docker configuration
uses the `pgvector/pgvector:pg18-trixie` image for this reason.

If `database update` reports that `knowledge_documents` already exists, the
database was migrated previously under a different migration ID. Do not delete
the tables or volume. Remove the newly generated duplicate migration and sync
the original migration files and model snapshot from the authoritative branch.
This preserves the schema, migration history, and ingested knowledge data.

Ingestion computes a SHA-256 hash for each source. An unchanged file with the same embedding model is skipped. Changed files are replaced, new files are added, and database documents whose source PDF no longer exists are removed.

## Inspect and search

```powershell
Invoke-RestMethod http://localhost:5175/api/knowledge/status
Invoke-RestMethod "http://localhost:5175/api/knowledge/search?query=hardware%20for%20local%20LLMs&limit=5"
```

The search response includes document title, category, relative source path, page number, chunk content, and cosine relevance. The assistant formats sources as `[Document title, p. N]` and is instructed never to invent citations.

## Inspect PostgreSQL and pgvector

Open an interactive `psql` session using the container's configured values:

```powershell
docker compose exec postgres sh -lc 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

Useful commands and queries:

```sql
\dx vector
\dt
\d+ knowledge_documents
\d+ knowledge_chunks

SELECT COUNT(*) AS documents FROM knowledge_documents;
SELECT COUNT(*) AS chunks FROM knowledge_chunks;

SELECT
    d."Title",
    d."Category",
    COUNT(c."Id") AS chunk_count
FROM knowledge_documents d
LEFT JOIN knowledge_chunks c ON c."DocumentId" = d."Id"
GROUP BY d."Id", d."Title", d."Category"
ORDER BY d."Category", d."Title";

SELECT
    d."Title",
    c."PageNumber",
    c."ChunkIndex",
    LEFT(c."Content", 250) AS content_preview,
    vector_dims(c."Embedding") AS dimensions
FROM knowledge_chunks c
JOIN knowledge_documents d ON d."Id" = c."DocumentId"
ORDER BY d."Title", c."ChunkIndex"
LIMIT 20;
```

Exit with `\q`.

## Boundaries

This module deliberately implements dense semantic retrieval only. Hybrid keyword/vector search, query rewriting, metadata filters, reranking, contextual compression, retrieval evaluation, and production ingestion authorization belong to later advanced RAG and security modules.
