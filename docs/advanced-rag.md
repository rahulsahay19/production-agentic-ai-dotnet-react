# Module 07: Advanced RAG

This module strengthens the initial dense-vector pipeline with hybrid retrieval and explicit grounding controls. It also renders model-authored Markdown safely in the React assistant.

## Retrieval pipeline

```text
Focused user question
  -> domain-aware query expansion and optional category routing
  -> OpenAI query embedding
  -> pgvector cosine candidates
  -> PostgreSQL full-text candidates
  -> score fusion and coverage-based reranking
  -> near-duplicate removal
  -> minimum relevance threshold
  -> evidence passages with immutable citation labels
  -> grounded answer or insufficient-evidence response
```

The final relevance score combines semantic similarity, normalized full-text rank, query-term coverage, and a small exact-phrase boost. Results below `Knowledge:MinimumRelevance` are not exposed to the model.

## Configuration

```json
{
  "Knowledge": {
    "CandidateMultiplier": 4,
    "MinimumRelevance": 0.35
  }
}
```

Docker Compose supports equivalent environment settings:

```dotenv
RAG_CANDIDATE_MULTIPLIER=4
RAG_MINIMUM_RELEVANCE=0.35
```

Increase the candidate multiplier when recall is more important than retrieval cost. Raise the minimum relevance when answers must be more conservative. Threshold changes do not require re-ingestion because stored embeddings are unchanged.

## Grounding contract

Every accepted passage contains an exact citation label such as `[NexaStore Returns and Refund Policy, p. 2]`. The system prompt requires the model to copy that label after the claim it supports and prohibits answering NexaStore document questions from general model knowledge.

When no result meets the threshold, the tool returns `INSUFFICIENT_EVIDENCE`. The assistant must disclose that the indexed documents do not establish the answer rather than guessing.

This is evidence-constrained generation, not a mathematical guarantee that every generated sentence is true. Retrieval evaluation and automated claim-level faithfulness measurement remain part of the evaluation and observability module.

## Markdown rendering

Assistant messages use `react-markdown` with `remark-gfm`. Headings, lists, emphasis, tables, links, blockquotes, inline code, and code blocks receive Tailwind styling. Raw HTML is disabled, and external links use `noopener noreferrer`.

User messages continue to render as plain text so user input is never interpreted as Markdown content.

## Verification

After starting the stack, try:

```text
Summarize the NexaStore return policy and cite every factual section.
Recommend an in-stock laptop using both the local LLM guide and live inventory.
What is NexaStore's policy for teleporting a laptop to Mars?
```

The first request should use hybrid document retrieval with citations. The second may combine RAG and commerce tools. The third should produce an insufficient-evidence response instead of an invented policy.
