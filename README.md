# NexaStore

**Computing for Developers, Creators & AI Builders**

NexaStore is a production-oriented .NET 10 modular-monolith e-commerce
application that will evolve incrementally into an agentic AI reference
platform. The current application provides a complete commerce API and a
responsive React storefront. AI, RAG, MCP, agents, and vector search remain
intentionally reserved for a later phase.

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

## Repository

```text
backend/        .NET solution projects
tests/          Unit and integration tests
frontend/       React storefront
knowledge-base/ Reserved for the future RAG corpus
docs/           Architecture and operating documentation
```
