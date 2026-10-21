# Prompting and Streaming

Module 03 evolves the local Ollama chat from a connectivity demonstration into a controlled conversational experience. It remains deliberately limited to prompting and streaming: there are no tools, RAG, agents, memory, or live commerce data in this module.

## Prompt architecture

The system prompt is owned by the server and composed by `AssistantPromptFactory`. It separates stable NexaStore behavior and safety boundaries from the response-style instruction selected for an individual request. User and assistant messages remain separate chat messages and cannot supply a system role.

Supported response styles:

- `concise`: a direct answer capped at a few paragraphs or bullets
- `balanced`: a focused answer with practical supporting detail
- `detailed`: headings, trade-offs, and a recommendation

Generation is intentionally predictable by default:

```json
{
  "AI": {
    "Temperature": 0.3,
    "MaxOutputTokens": 800
  }
}
```

Both values are server-controlled and validated at startup. Clients cannot override model-level generation limits.

## Streaming protocol

`POST /api/assistant/chat/stream` uses Server-Sent Events and emits:

1. `started` with the request ID, model, and selected response style
2. ordered `delta` events containing generated text chunks
3. `done` with the request ID, chunk count, and server elapsed time
4. `error` when the provider fails after streaming has started

Example request:

```json
{
  "responseStyle": "balanced",
  "messages": [
    {
      "role": "user",
      "content": "Explain RAM versus VRAM for a local AI workstation."
    }
  ]
}
```

The React client parses fragmented SSE frames, renders tokens incrementally, supports cancellation through `AbortController`, reports elapsed generation time, and can clear the in-browser conversation. Conversation history is sent by the browser for the current panel session only; durable memory is outside this module.

## Boundaries

- Maximum 20 messages per request
- Maximum 4,000 characters per message
- Final message must have the `user` role
- Only `user` and `assistant` roles are accepted
- Only the three documented response styles are accepted
- The server always supplies the system prompt

These controls constrain request shape and improve consistency. They do not make model output inherently factual, so the UI continues to disclose that live store grounding is unavailable.
