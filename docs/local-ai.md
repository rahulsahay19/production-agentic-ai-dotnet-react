# Local AI with Ollama

Phase 02 introduces conversational AI without RAG, tools, MCP, or agents. NexaStore uses `Microsoft.Extensions.AI.IChatClient` as its provider-neutral boundary and Ollama as the local implementation.

## Start the complete stack

```powershell
docker compose up -d --build
```

The first run downloads the configured Ollama image and model, so it can take several minutes. Model data is retained in the `nexastore-ollama` Docker volume.

Services:

- Storefront: `http://localhost:5173`
- API and Swagger: `http://localhost:5175`
- Ollama API: `http://localhost:11434`
- PostgreSQL: `localhost:5432`

Open the storefront and select **Ask Nexa AI**. Responses stream from the local model through the ASP.NET Core API and Nginx.

## Model configuration

The default model is intentionally moderate for local development:

```env
OLLAMA_MODEL=qwen3.5:4b
```

Change `OLLAMA_MODEL` in the root `.env`, then recreate the model initializer and API:

```powershell
docker compose up -d --force-recreate ollama-model nexastore-api
```

Select a model that fits the machine. Larger models generally improve responses but require more memory and are slower without GPU acceleration.

## Run Ollama outside Docker

When the API runs from Visual Studio or `dotnet run`, it uses:

```json
{
  "AI": {
    "Enabled": true,
    "Provider": "Ollama",
    "Endpoint": "http://localhost:11434",
    "Model": "qwen3.5:4b"
  }
}
```

Install and start Ollama separately, then pull the configured model:

```powershell
ollama pull qwen3.5:4b
```

## API

Check the configured provider:

```http
GET /api/assistant/status
```

Stream a conversation with Server-Sent Events:

```http
POST /api/assistant/chat/stream
Content-Type: application/json

{
  "messages": [
    {
      "role": "user",
      "content": "Explain RAM versus VRAM for local AI workloads."
    }
  ]
}
```

The server owns the system prompt. Client messages are limited to `user` and `assistant` roles, 20 messages per request, and 4,000 characters per message.

## Architectural boundary

This phase can explain general computing concepts and clarify requirements. It intentionally cannot access live products, inventory, customers, orders, policies, or documents. Those capabilities arrive through structured tools and RAG in later phases. The model must not be treated as a source of current store facts.

Future Azure integration will register a different `IChatClient`; application contracts and the React streaming protocol remain unchanged.
