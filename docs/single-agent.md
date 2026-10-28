# NexaStore Support & Shopping Agent

Module 09 converts the tool-capable assistant into one explicitly defined agent. The **NexaStore Support & Shopping Agent** owns a customer request from understanding through completion and uses the MCP capabilities introduced in Module 08.

This is one agent with one identity, one decision policy, and one bounded execution loop. It does not simulate a team of specialist agents.

## Agent responsibility

The agent can:

- discover shopping requirements and recommend matching live products;
- compare current specifications, prices, categories, and inventory;
- answer policy, support, buying-guide, program, manual, and catalogue questions from indexed documents;
- combine document guidance with current catalog evidence in a single answer;
- explain stable general computing concepts without unnecessary tool calls;
- ask one focused clarification when a missing constraint materially changes the result.

It cannot mutate carts, orders, inventory, customers, accounts, refunds, or any other state. It cannot browse arbitrary websites or delegate work to another agent.

## Execution loop

```text
Customer request
      |
      v
Observe conversation and available MCP tools
      |
      v
Decide: answer, clarify, or invoke the smallest safe tool set
      |
      v
Act: catalog / inventory / hybrid-RAG MCP call
      |
      v
Observe structured result and decide whether more evidence is needed
      |
      v
Return one grounded answer and stop
```

`Microsoft.Extensions.AI` function-invocation middleware executes the tool loop. The agent prompt supplies its identity, goals, routing policy, evidence rules, and stopping behavior. This keeps model-provider code, orchestration policy, and business capabilities separate.

## Decision policy

| User need | Expected action |
| --- | --- |
| General computing explanation | Answer directly |
| Current product, price, specification, or category | Use commerce MCP tools |
| Current stock or requested quantity | Use inventory MCP tool |
| Policy, support, guide, program, manual, or catalogue claim | Use grounded knowledge search |
| Support guidance followed by a product recommendation | Retrieve document guidance, then verify live products and inventory |
| Material requirement is missing | Ask one focused clarification |
| Evidence is insufficient | State the limitation; do not invent an answer |

The agent stops when it has enough evidence to answer. It is instructed not to repeat equivalent tool calls or invoke tools only to demonstrate activity.

## Configuration

```json
"Agent": {
  "Name": "NexaStore Support & Shopping Agent",
  "MaximumToolIterations": 5
}
```

Docker Compose supports:

```dotenv
AGENT_MAXIMUM_TOOL_ITERATIONS=5
```

The allowed range is 1–10. Five provides enough room for a grounded mixed workflow while bounding cost and preventing unproductive loops. The function-invocation middleware also terminates on unknown tool calls and does not permit concurrent invocations.

## Inspect the agent

```powershell
Invoke-RestMethod http://localhost:5175/api/assistant/agent
```

The response publishes the agent's identity, role, orchestration strategy, iteration limit, goals, and boundaries. Provider status at `/api/assistant/status` includes `single-agent`, `intent-routing`, and `bounded-tool-loop` capabilities.

## UI test scenarios

Direct answer without NexaStore data:

```text
What is the difference between RAM and VRAM?
```

Live shopping workflow:

```text
Show me AI-ready laptops with at least 16 GB VRAM and check whether two units are available.
```

Grounded support workflow:

```text
What hardware does the local LLM guide recommend? Cite the source.
```

Mixed support and shopping workflow:

```text
Based on the local LLM guide, recommend a currently available NexaStore laptop for running models locally and explain why it fits.
```

Clarification behavior:

```text
Recommend the best laptop for me.
```

Verify that current NexaStore claims come from tools, document claims carry exact citations, missing evidence is disclosed, and no cart/order mutation is claimed.

## Observability

Each request is logged as an agent run with a unique run ID, the configured agent name, provider, and conversation-message count. Existing SSE events continue to expose request lifecycle, streamed deltas, and tool activity without revealing private model reasoning.

## Module boundary

This module delivers a single model-driven agent. It does not add JEV governance, multiple specialized agents, durable memory, human approval, or agent-to-agent communication. Those remain separate modules in the course plan.
