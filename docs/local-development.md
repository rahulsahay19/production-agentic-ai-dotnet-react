# Local Development and Debugging

This is the recommended course-recording and breakpoint-debugging topology:

```text
Browser
  |
  v
React/Vite on http://localhost:5173
  |  proxies /api and /health
  v
NexaStore API in Visual Studio on http://localhost:5175
  |                              |
  |                              +--> self-hosted MCP endpoint /mcp
  v
PostgreSQL + pgvector in Docker on localhost:5432
```

Do not keep the Compose frontend running in this mode. Its Nginx configuration
routes `/api` to `http://nexastore-api:8080`, which targets the API container and
cannot reach breakpoints in Visual Studio.

## 1. Stop application containers and start PostgreSQL

From the repository root:

```powershell
docker compose stop nexastore-api nexastore-web
docker compose up -d postgres
docker compose ps
```

PostgreSQL should be healthy and published on `localhost:5432`.

## 2. Configure local secrets

Visual Studio and `dotnet run` do not automatically read Docker Compose's `.env`
file. Keep credentials out of `appsettings.json` and configure .NET user secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:NexaStore" "Host=localhost;Port=5432;Database=nexastore;Username=nexastore;Password=NexaStore_Local_2026!" --project backend/NexaStore.Api
dotnet user-secrets set "AI:Providers:OpenAI:ApiKey" "<YOUR_OPENAI_API_KEY>" --project backend/NexaStore.Api
```

The local configuration uses:

```text
API URL:     http://localhost:5175
MCP URL:     http://localhost:5175/mcp
```

Normal RAG queries read previously ingested chunks and embeddings directly from
PostgreSQL/pgvector. A filesystem `Knowledge:RootPath` override is not required
for the local demo. It is needed only when deliberately running PDF ingestion.

Never use `http://localhost:8080/mcp` when the API runs in Visual Studio. Port
`8080` is the API container's internal port and is only appropriate inside that
container.

## 3. Apply migrations on a fresh database

The Visual Studio profile intentionally does not apply migrations at startup.
For a new PostgreSQL volume, run:

```powershell
dotnet ef database update --project backend/NexaStore.Infrastructure --startup-project backend/NexaStore.Api --connection "Host=localhost;Port=5432;Database=nexastore;Username=nexastore;Password=NexaStore_Local_2026!"
```

Do not create a new migration unless the EF model intentionally changed. If the
database was already initialized by Docker Compose, this command simply confirms
that all committed migrations are applied.

## 4. Run the API in Visual Studio

Select `NexaStore.Api` as the startup project and run the `api` profile. Verify:

```powershell
Invoke-RestMethod http://localhost:5175/health
Invoke-RestMethod http://localhost:5175/api/assistant/status
Invoke-RestMethod http://localhost:5175/api/mcp/status
```

Swagger is available at `http://localhost:5175/swagger`.

## 5. Run React locally

In a second terminal:

```powershell
cd frontend\nexastore-web
npm.cmd install
npm.cmd run dev
```

Open `http://localhost:5173`. Vite proxies `/api` and `/health` to
`http://localhost:5175`, so frontend requests reach the Visual Studio debugger.
Use `npm.cmd` when PowerShell execution policy prevents `npm.ps1` from running.

## 6. Inspect MCP

Start the web Inspector:

```powershell
npx --yes @modelcontextprotocol/inspector@2.5.0 --web --server-url http://localhost:5175/mcp --transport http --protocol-era legacy
```

Or list tools from the terminal:

```powershell
npx --yes @modelcontextprotocol/inspector@2.5.0 --cli --transport http --server-url http://localhost:5175/mcp --method tools/list
```

Invoke `search_products` with:

```json
{
  "category": "AI Laptops",
  "minimumVramGb": 16,
  "aiReady": true
}
```

The expected result contains `NexaBook AI Studio 16` and an explicit
`"error": null` property.

## 7. Debugging timeouts and cancellation

MCP Inspector has a bounded request timeout. If Visual Studio remains paused at
a breakpoint, Inspector may cancel the request and the cancellation token flows
through MCP, application services, EF Core, and PostgreSQL. Visual Studio can
then break on `OperationCanceledException` inside otherwise-correct repository
code.

For breakpoint-heavy debugging:

1. Open the Inspector server settings.
2. Set `requestTimeout` to `300000` milliseconds.
3. Disconnect and reconnect the Inspector.
4. Disable break-on-thrown for `OperationCanceledException` and
   `TaskCanceledException` when investigating normal client cancellation.
5. Place the breakpoint inside the target tool, not on MCP client initialization.

For the VRAM example, the expected tool arguments are:

```text
keyword       = null
category      = "AI Laptops" or null
minimumVramGb = 16
aiReady       = true
```

The agent prompt and tool schema instruct the model not to place complete user
requests or structured requirements inside `keyword`.

## Troubleshooting checklist

| Symptom | Check |
| --- | --- |
| Visual Studio breakpoint is never reached | Stop `nexastore-web`; run Vite locally |
| API cannot connect to PostgreSQL | PostgreSQL is healthy and local connection includes username/password |
| MCP discovery fails locally | `Mcp:ServerUrl` is `http://localhost:5175/mcp` |
| MCP tool times out while stepping | Raise Inspector `requestTimeout` and continue execution before it expires |
| Inspector says `error` is required | Rebuild/restart API and reconnect Inspector so it reloads the fixed tool schema |
| Assistant request reports empty content | Clear the old conversation and run the frontend containing empty-placeholder cleanup |
