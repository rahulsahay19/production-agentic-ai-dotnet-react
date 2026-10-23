# AI tools

Module 04 adds controlled function calling to the local Ollama assistant. The implementation uses `Microsoft.Extensions.AI` tool declarations and its function-invocation middleware, keeping the tool contracts independent from Ollama.

## Available tools

| Tool | Purpose | Limits |
|---|---|---|
| `search_products` | Search current products using requirements such as budget, RAM, VRAM, and AI readiness | Returns at most five products |
| `get_product_details` | Read specifications and capabilities for a product returned by search | Requires a product ID |
| `list_product_categories` | Read current category names and slugs | Read-only |
| `check_inventory` | Check current availability for a product | Quantity must be 1–20 |

The tools call application services rather than repositories or SQL directly. Existing validation, domain rules, logging, and persistence boundaries therefore remain authoritative.

## Execution flow

1. The API supplies the server-owned system prompt and the current conversation.
2. `ChatOptions.Tools` advertises only the four allowlisted functions.
3. Ollama decides whether a tool is needed and returns a structured function call.
4. `FunctionInvokingChatClient` validates and invokes the matching local function.
5. The structured result is returned to the model for its final natural-language answer.
6. A `tool` SSE event makes each successful lookup visible in the React conversation.

The invocation pipeline permits at most five model round trips, stops on unknown functions, runs tools sequentially, and does not send exception details back to the model.

## Safety boundary

This module intentionally exposes public, read-only commerce facts only. It does not expose:

- Cart changes or checkout
- Customer or account information
- Order lookup or cancellation
- Policy/document retrieval
- Arbitrary database queries
- URLs, files, shell commands, or external network access

Customer and order tools require authenticated user identity, authorization, and approval behavior that belongs in later security and human-in-the-loop modules. RAG and document grounding begin in module 05.

## Streaming events

In addition to module 03 events, the API can emit:

```text
event: tool
data: {"name":"search_products","label":"Searched the live product catalog"}
```

Tool arguments and raw database records are not included in the event. The client receives only a safe activity label; the model receives the minimal structured result needed to answer.
