# Product Discovery Copilot

**Status:** Scaffolded for the 2026-06-06T14:05:16.188-04:00 agentic modernization plan.

## Derived from
01-Semantic Search.

## What this scenario will demonstrate
- Natural-language product discovery.
- Grounded explanations based on catalog search results.
- How grounding works in code: catalog embedded at startup, question embedded at query time,
  `Score > 0.3` similarity gate, and a prompt built only from matched products.

## Session format
For the agentic modernization session this is presented as a **code walkthrough — no app run**.
The grounding logic is read directly from
[`src/Products/Memory/MemoryContext.cs`](src/Products/Memory/MemoryContext.cs). See the demo
script `demo-14scenario.md` in the session package.

## Session docs
See the shared session package at [docs/26 06 16 NET Agentic Modernization](../../docs/26%2006%2016%20NET%20Agentic%20Modernization/README.md).

## Next step
Optionally add a dedicated discovery/copilot UI flow; the core grounding pipeline already
lives in `MemoryContext.cs`.
