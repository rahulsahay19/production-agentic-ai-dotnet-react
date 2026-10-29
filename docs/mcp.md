# Model Context Protocol (MCP)

Module 08 turns NexaStore's existing read-only AI functions into protocol-level capabilities. The API is both an MCP server and an MCP client: external hosts can discover NexaStore through `/mcp`, while the chat assistant discovers and invokes the same tools through MCP instead of directly referencing their implementations.

## Architecture

```text
React assistant
      |
      v
NexaStore chat API -> MCP client -> /mcp (Streamable HTTP)
                                      |
                       +--------------+--------------+
                       |                             |
                  MCP tools                    MCP resources
                       |                             |
          catalog / inventory / RAG       nexastore://capabilities
```

`Microsoft.Extensions.AI.IChatClient` remains the model boundary. MCP changes how application capabilities are discovered and invoked; it does not replace OpenAI, Ollama, prompting, streaming, or retrieval.

## Published capabilities

The server exposes these allowlisted tools:

| Tool | Purpose |
| --- | --- |
| `search_products` | Search the live catalog with bounded filters |
| `get_product_details` | Read specifications for one product |
| `list_product_categories` | Read available catalog categories |
| `check_inventory` | Check bounded inventory availability |
| `search_knowledge_base` | Run grounded hybrid retrieval over indexed documents |

All tools are annotated as read-only, non-destructive, idempotent, and closed-world. They reuse the existing validated application services rather than duplicating commerce or RAG logic.

The `nexastore://capabilities` resource describes the server's purpose, grounding sources, tools, and safety boundary as JSON.

## Configuration

```json
"Mcp": {
  "Enabled": true,
  "UseForAssistant": true,
  "ServerUrl": "http://localhost:5175/mcp",
  "ConnectionTimeoutSeconds": 15,
  "AllowedTools": [
    "search_products",
    "get_product_details",
    "list_product_categories",
    "check_inventory",
    "search_knowledge_base"
  ]
}
```

For the course's local development topology, the API and its MCP endpoint both
run locally. Keep `Mcp:ServerUrl` set to `http://localhost:5175/mcp`.

Set `MCP_USE_FOR_ASSISTANT=false` to keep publishing the MCP server while temporarily routing the assistant through the in-process tool provider. Set `MCP_ENABLED=false` to disable the MCP endpoint and use local tools.

## Run and verify

Start PostgreSQL, then run the API in Visual Studio and React through Vite as
described in the local development guide:

```powershell
docker compose up -d postgres
```

Inspect the configured endpoint and allowlist:

```powershell
Invoke-RestMethod http://localhost:5175/api/mcp/status
```

Open the official protocol inspector:

```powershell
npx --yes @modelcontextprotocol/inspector@2.5.0 --web --server-url http://localhost:5175/mcp --transport http --protocol-era legacy
```

For a terminal-only discovery check:

```powershell
npx --yes @modelcontextprotocol/inspector@2.5.0 --cli --transport http --server-url http://localhost:5175/mcp --method tools/list
```

In the inspector, list tools, list resources, read `nexastore://capabilities`, and invoke `search_products` with:

```json
{
  "minimumRamGb": 64,
  "minimumVramGb": 16,
  "aiReady": true
}
```

Successful structured results deliberately include `"error": null`. MCP output schemas
require constructor-backed record properties to be present even when nullable, so the
tool contracts force these properties to serialize instead of omitting them.

When debugging a tool call, do not leave Visual Studio paused for a minute: the Inspector
has a bounded per-request timeout and will cancel the call. Increase the server's
`requestTimeout` to `300000` milliseconds in Inspector settings for breakpoint-heavy
sessions, then disconnect and reconnect. Visual Studio may otherwise break on the
resulting `OperationCanceledException`; cancellation is expected after the client
abandons a timed-out request.

For the complete PostgreSQL-in-Docker plus local API/frontend workflow, see the
[local debugging guide](local-development.md).

The existing React assistant is also an end-to-end MCP test. Try:

- `Show me AI-ready laptops with at least 16 GB VRAM.`
- `What hardware does the local LLM guide recommend? Cite the source.`
- `List the current product categories.`

The assistant lazily creates an MCP client per request scope, discovers the server tools, keeps only the configured allowlist, and fails closed if a required tool is absent. The provider is disposed with the request scope.

## Persistence and migrations

This module adds no database entities or migrations. Existing products, inventory, documents, chunks, embeddings, and advanced-RAG indexes remain unchanged. PDF ingestion does not need to be repeated.

## Security boundary

Only public, read-only operations are published. Cart mutation, ordering, customer data, administrative ingestion, filesystem access, and arbitrary SQL are not exposed. The MCP endpoint is intentionally unauthenticated for the local course environment; authentication, authorization, rate limiting, audit controls, and adversarial evaluation belong to the security and observability module before public deployment.

## Module boundary

This module implements MCP transport, discovery, tools, resources, allowlisting, and integration testing. It does not introduce autonomous planning, agent identity, durable memory, approvals, multi-agent delegation, or human-in-the-loop execution. Those remain separate modules so the protocol layer can be understood and validated independently.
