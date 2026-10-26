# NexaStore

**Computing for Developers, Creators & AI Builders**

NexaStore is a production-oriented .NET 10 modular-monolith e-commerce
application that evolves incrementally into an agentic AI reference platform.
The current application combines the conventional commerce foundation with a
provider-selectable conversational AI integration. OpenAI is the cloud-first
default and Ollama remains available as a local fallback. The
assistant can query public commerce data through read-only tools and retrieve
source-grounded answers from a PostgreSQL/pgvector knowledge base. MCP and
agents remain intentionally reserved for later modules.

## Module 02: local LLM

- `Microsoft.Extensions.AI.IChatClient` provider abstraction
- Ollama local model integration
- Server-Sent Events streaming API
- Multi-turn React assistant experience with cancellation
- Server-owned safety and capability-boundary prompt
- Docker-managed model provisioning and persistent model storage

See [local AI documentation](docs/local-ai.md) for configuration, model selection, and manual verification.

## Module 03: prompting and streaming

- Composable, server-owned system and response-style prompts
- Concise, balanced, and detailed response modes
- Server-controlled temperature and output-token limits
- Structured `started`, `delta`, `done`, and `error` SSE events
- Ordered chunks, request correlation, elapsed-time reporting, and cancellation
- Clear-conversation and response-style controls in the React assistant

See [prompting and streaming documentation](docs/prompting-streaming.md) for the prompt architecture, streaming protocol, and module boundaries.

## Module 04: AI tools

- Automatic function invocation through `Microsoft.Extensions.AI`
- Live product search, product-detail, category, and inventory tools
- Strongly typed tool inputs and compact structured results
- Read-only allowlist with bounded catalog results and inventory quantities
- Tool-loop iteration and error limits
- Visible tool activity in the streaming React experience

See [AI tools documentation](docs/ai-tools.md) for the tool contracts, execution flow, safeguards, and module boundaries.

## Module 05: cloud LLM provider

- Direct OpenAI integration through `Microsoft.Extensions.AI`
- OpenAI cloud-first configuration with Ollama fallback
- Shared streaming and function-invocation pipeline across providers
- Startup validation for provider, credentials, endpoint, model, and timeout
- Server-only secret handling and provider-aware frontend status
- Docker Compose profiles so Ollama starts only when requested

See [cloud LLM provider documentation](docs/cloud-llm-provider.md) for secure configuration and provider switching.

## Module 06: OpenAI RAG with pgvector

- PDF extraction with page metadata across the six knowledge domains
- Overlapping, page-aware chunks and batched OpenAI embeddings
- PostgreSQL `vector(1536)` storage with an HNSW cosine index
- SHA-256 idempotent ingestion that skips unchanged documents
- Semantic-search API plus an allowlisted `search_knowledge_base` AI tool
- Document-title and page-number citations in assistant answers

See [RAG and pgvector documentation](docs/rag-pgvector.md) for ingestion, configuration, APIs, and module boundaries.

## Module 07: advanced RAG

- Hybrid dense-vector and PostgreSQL full-text retrieval
- Domain-aware query expansion and metadata/category routing
- Candidate reranking, near-duplicate removal, and relevance thresholds
- Evidence-bound citations and explicit insufficient-evidence responses
- Safe GitHub-flavored Markdown rendering in the streaming React assistant

See [advanced RAG documentation](docs/advanced-rag.md) for retrieval scoring, grounding behavior, and verification.

## Backend status

- .NET 10 ASP.NET Core REST API
- EF Core 10 with PostgreSQL
- Structured product search
- Transactional cart-to-order workflow
- Optimistic inventory/order concurrency through PostgreSQL `xmin`
- FluentValidation and ProblemDetails
- Serilog structured logging
- OpenAPI and Swagger UI
- Liveness, readiness, and combined health checks
- Unit, persistence, workflow, and HTTP integration tests

See [backend documentation](docs/backend.md) for configuration and execution.

## Frontend status

- React 19 and TypeScript 6 on Vite 8
- Tailwind CSS 4 design system and responsive premium storefront
- Product discovery, filters, product details, cart, checkout, and order history
- TanStack Query server-state management and typed Axios API boundary
- React Router route architecture with loading, error, empty, and 404 states
- Vitest and Testing Library coverage

See [frontend documentation](frontend/nexastore-web/README.md) for setup and quality commands.

## Run the complete stack

```powershell
docker compose up -d --build
```

Docker Compose applies pending EF Core migrations automatically because the API
container sets `Database__ApplyMigrationsOnStartup=true`. This includes the
`AddKnowledgeBaseVectors` migration, which enables the PostgreSQL `vector`
extension and creates the knowledge tables and HNSW index. Compose applies
existing migration files; it does not generate migrations.

When running the API outside Docker, apply the migrations manually from the
repository root. PostgreSQL must already be running, and the connection must
include the credentials from `.env`:

```powershell
docker compose up -d postgres
dotnet ef database update --project backend/NexaStore.Infrastructure --startup-project backend/NexaStore.Api --connection "Host=localhost;Port=5432;Database=nexastore;Username=nexastore;Password=<POSTGRES_PASSWORD>"
```

Create a migration only after intentionally changing the EF model, then commit
both generated migration files and the updated model snapshot. Multiple
checkouts must use the same migration files and migration ID; do not generate
another migration for an already-created schema change.

- Storefront: `http://localhost:5173`
- API and Swagger: `http://localhost:5175`
- PostgreSQL: `localhost:5432`
- Ollama (optional profile): `http://localhost:11434`

## Repository

```text
backend/        .NET solution projects
tests/          Unit and integration tests
frontend/       React storefront
knowledge-base/ Source PDF corpus for RAG ingestion
docs/           Architecture and operating documentation
```
