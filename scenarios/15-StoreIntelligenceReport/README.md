# Store Intelligence Report

**Status:** Implemented for the agentic modernization session (Demo 3).

## Derived from
01-Semantic Search.

## What this scenario demonstrates
- Turning the app's own **search signals** (including failed/no-result searches) into a
  business-ready **Store Intelligence Report**.
- A structured report — executive summary, top customer intents, searches with no results,
  product opportunities, operational issues, recommended actions.
- AI narrative via the same Azure OpenAI `IChatClient` as Demo 1, with a **deterministic
  fallback** (`source: ai | fallback`) so the demo never blocks.
- Signals are seeded at startup, so the report is never empty before any live search.

## How it works
- Keyword (`ProductApiActions.SearchAllProducts`) and semantic (`ProductAiActions.AISearch`)
  searches record a `StoreSignal` into `Products/Intelligence/StoreSignalStore.cs`
  (in-memory, seeded with 8 samples).
- `Products/Intelligence/StoreIntelligenceReportService.cs` aggregates the signals and produces
  the report (AI summary + deterministic fallback).
- `Products/Endpoints/IntelligenceEndpoints.cs` exposes
  `GET /api/intelligence/signals` and `GET /api/intelligence/report`.
- The `store` **Store Intelligence** page (`Store/Components/Pages/StoreIntelligence.razor`,
  route `/intelligence`) renders the signals table and the generated report.

## Run it
```pwsh
cd src/eShopAppHost
aspire start --non-interactive
```
Open the `store` endpoint, go to **Store Intelligence**, click **Generate report**.

## Session docs
See the shared session package at [docs/26 06 16 NET Agentic Modernization](../../docs/26%2006%2016%20NET%20Agentic%20Modernization/README.md)
and the demo script [demo-15scenario.md](../../docs/26%2006%2016%20NET%20Agentic%20Modernization/demo-15scenario.md).

## Optional next steps
- Persist signals to SQL (currently in-memory).
- Add cart/checkout-event signals in addition to search signals.
