# Cloud LLM provider

Module 05 makes the NexaStore assistant provider-selectable. OpenAI is the cloud-first default, while Ollama remains available for local development. Both providers sit behind `Microsoft.Extensions.AI.IChatClient` and share the same streaming, prompt, cancellation, and read-only tool pipeline introduced in Modules 02–04. The OpenAI adapter uses `/v1/responses`; Ollama continues to use its native chat endpoint.

This module does not add RAG, embeddings, agents, memory, or new commerce tools.

## OpenAI configuration

Never commit an API key. The provider, endpoint, model, and timeout are defined
in `appsettings.json`; store only the secret key in .NET user secrets:

```powershell
dotnet user-secrets set "AI:Providers:OpenAI:ApiKey" "your-secret-key" --project backend/NexaStore.Api
```

Start PostgreSQL, then run the API in Visual Studio and the frontend through
local Vite as described in the local development guide:

```powershell
docker compose up -d postgres
cd frontend\nexastore-web
npm.cmd run dev
```

The API key is loaded only by the local server process. It is never sent to or
stored by the React application.

Alternatively, set an environment variable in the API process instead of placing
the key in `appsettings.json`:

```powershell
$env:AI__Providers__OpenAI__ApiKey = "your-secret-key"
```

Only one of those approaches is required.

## Ollama fallback

Configure the locally running API for Ollama:

```dotenv
AI_PROVIDER=Ollama
OLLAMA_ENDPOINT=http://localhost:11434
OLLAMA_MODEL=qwen3.5:4b
```

Then start the optional Ollama profile:

```powershell
docker compose --profile ollama up -d ollama ollama-model
```

Use `AI__Providers__Ollama__Endpoint=http://localhost:11434` for the local API.

## Provider boundary

`AddAi` validates configuration at startup, creates the selected provider client, and wraps it once with the shared function-invocation middleware. For OpenAI it adapts `ResponsesClient` to `IChatClient`, so reasoning models and function tools use the Responses API. `NexaStoreSupportShoppingAgent` therefore contains no provider SDK logic. Provider credentials remain an infrastructure concern, and switching provider does not alter the application contract or frontend streaming protocol.

OpenAI receives an explicit medium reasoning effort and no sampling temperature. The OpenAI options object is built without assigning `Temperature`, because reasoning models reject that sampling parameter. Ollama receives the configured temperature and no OpenAI reasoning settings.

## Verification

The automated tests use a fake assistant stream and a non-production placeholder key. They verify configuration and HTTP contracts without making paid external API calls. A real OpenAI response is a manual integration check after supplying your own key.
