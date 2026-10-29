# NexaStore Web

The NexaStore customer storefront, built with React, TypeScript, Vite, Tailwind CSS, TanStack Query, React Router, Axios, and Vitest.

## Run locally

The Vite development server proxies `/api` and `/health` to the backend at `http://localhost:5175`.

```powershell
npm.cmd install
npm.cmd run dev
```

Use `npm.cmd` in Windows PowerShell when the machine's execution policy blocks
`npm.ps1`. When debugging the API in Visual Studio, stop the Compose frontend
and API first; otherwise the Nginx frontend proxies to the API container rather
than the Visual Studio process. See the [local debugging guide](../../docs/local-development.md).

Copy `.env.example` to `.env.local` when the API or demo customer differs from the defaults.

## Quality commands

```powershell
npm run typecheck
npm run lint
npm test
npm run build
```

## Structure

- `src/app` — application providers and routing
- `src/components` — shared layout and UI primitives
- `src/features` — feature-owned catalog, cart, order, and home modules
- `src/config` — runtime configuration boundary
- `src/lib` — HTTP and formatting utilities
- `src/pages` — cross-feature route pages
- `src/test` — shared test setup

The customer ID defaults to the deterministic development customer seeded by the API. Authentication will replace this development bridge in a later phase.

## Local AI assistant

The header's **Ask Nexa AI** action opens the NexaStore Support & Shopping Agent.
It streams responses, renders Markdown, supports cancellation and response styles,
discovers read-only commerce and knowledge tools through MCP, and combines live
catalogue/inventory data with grounded RAG citations. Cart, order, customer, and
other write operations remain outside the agent's capability boundary.

## Development topology

Run PostgreSQL/pgvector in Docker, the API in Visual Studio, and this React
application through the local Vite server. Vite proxies `/api` to
`http://localhost:5175`, allowing requests to reach API breakpoints directly.
