# NexaStore

**Computing for Developers, Creators & AI Builders**

NexaStore is a production-oriented .NET 10 modular-monolith e-commerce
application that evolves incrementally into an agentic AI reference platform.
The current application combines the conventional commerce foundation with a
provider-neutral local conversational AI integration powered by Ollama. RAG,
tools, MCP, and agents remain intentionally reserved for later phases.

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

- Storefront: `http://localhost:5173`
- API and Swagger: `http://localhost:5175`
- PostgreSQL: `localhost:5432`
- Ollama: `http://localhost:11434`

## Repository

```text
backend/        .NET solution projects
tests/          Unit and integration tests
frontend/       React storefront
knowledge-base/ Reserved for the future RAG corpus
docs/           Architecture and operating documentation
```
