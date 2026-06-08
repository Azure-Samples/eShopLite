# A2A Store Operations Network

**Status:** Remapped to store-operations agent roles (Catalog / Promotions / BusinessInsights) and validated live. Fixed the agents crashing on .NET 10 (`System.TypeLoadException` from Swashbuckle) — Swagger removed from the agent services since it is dev-only and unused by the demo. A2A search now returns enriched product cards (Stock / Promotions / Reviews). Demo script: `docs/26 06 16 NET Agentic Modernization/demo-17scenario.md`. Plan-B walkthrough assets: `docs/26 06 16 NET Agentic Modernization/screenshots/scenario17-*`.

## Derived from
10 - A2A Network.

## What this scenario will demonstrate
- Specialized agents collaborating around the store.
- Catalog, observability, and business-insights responsibilities.
- A path to hosted agents or local NeMo-style integration.

## Session docs
See the shared session package at [docs/26 06 16 NET Agentic Modernization](../../docs/26%2006%2016%20NET%20Agentic%20Modernization/README.md).

## Next step
Add the orchestrator flow and agent cards.
