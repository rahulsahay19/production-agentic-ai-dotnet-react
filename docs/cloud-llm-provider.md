# Cloud LLM provider

Module 05 makes the NexaStore assistant provider-selectable. OpenAI is the cloud-first default, while Ollama remains available for local development. Both providers sit behind `Microsoft.Extensions.AI.IChatClient` and share the same streaming, prompt, cancellation, and read-only tool pipeline introduced in Modules 02–04. The OpenAI adapter uses `/v1/responses`; Ollama continues to use its native chat endpoint.

This module does not add RAG, embeddings, agents, memory, or new commerce tools.

## OpenAI configuration

Never commit an API key. Copy `.env.example` to `.env`, keep `.env` ignored, and configure:

```dotenv
AI_PROVIDER=OpenAI
OPENAI_ENDPOINT=https://api.openai.com/v1
OPENAI_MODEL=gpt-5.6-terra
OPENAI_API_KEY=your-secret-key
AI_REQUEST_TIMEOUT_SECONDS=120
```

Start the application:

```powershell
docker compose up -d --build
```

The API key is injected only into the server container. It is never sent to or stored by the React application.

For local `dotnet run`, use .NET user-secrets or an environment variable instead of placing the key in `appsettings.json`:

```powershell
dotnet user-secrets set "AI:Providers:OpenAI:ApiKey" "your-secret-key" --project backend/NexaStore.Api
$env:AI__Providers__OpenAI__ApiKey = "your-secret-key"
```

Only one of those approaches is required.

## Ollama fallback

Set these values in `.env`:

```dotenv
AI_PROVIDER=Ollama
OLLAMA_ENDPOINT=http://ollama:11434
OLLAMA_MODEL=qwen3.5:4b
```

Then start the optional Ollama profile:

```powershell
docker compose --profile ollama up -d --build
```

When the API runs directly on the host, use `AI__Providers__Ollama__Endpoint=http://localhost:11434`.

## Provider boundary

`AddAi` validates configuration at startup, creates the selected provider client, and wraps it once with the shared function-invocation middleware. For OpenAI it adapts `ResponsesClient` to `IChatClient`, so reasoning models and function tools use the Responses API. `NexaAssistantService` therefore contains no provider SDK logic. Provider credentials remain an infrastructure concern, and switching provider does not alter the application contract or frontend streaming protocol.

OpenAI does not receive the Ollama temperature setting. This preserves reasoning-model compatibility while retaining the course's existing Ollama sampling behavior.

## Verification

The automated tests use a fake assistant stream and a non-production placeholder key. They verify configuration and HTTP contracts without making paid external API calls. A real OpenAI response is a manual integration check after supplying your own key.
