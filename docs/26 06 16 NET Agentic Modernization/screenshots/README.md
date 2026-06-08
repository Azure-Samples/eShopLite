# Plan-B walkthrough assets (scenarios 13–18)

Captured live via `aspire start` + Playwright (Microsoft Edge, 1600×1000) against each scenario's
running Store and Aspire Dashboard. Use these if a live demo fails — each scenario has step-by-step
JPGs plus two animated GIFs:

- `scenarioNN-walkthrough.gif` — fast loop (~1.8 s/frame).
- `scenarioNN-walkthrough-long.gif` — slow loop, **3 s/frame with the final frame held for 15 s** (good for unattended/standby playback).

> Note: `playwright-cli screenshot` writes PNG even when the filename ends in `.jpg`; frames here were
> converted to real JPEG with Pillow.

## Index

| Scenario | Demo | Steps | Frames | GIFs |
|---|---|---:|---|---|
| 13 — Observability Assistant (Foundry Local) | Demo 2 (live) | 6 | dashboard → store → run searches → **search result** → observability-assistant page → analysis findings (`source: foundry-local`) | `scenario13-walkthrough.gif`, `scenario13-walkthrough-long.gif` |
| 14 — Product Discovery Copilot | Demo 1 (code walkthrough) | 5 | dashboard → store → semantic search → grounded AI answer → **Aspire traces of the search calls** (store → aisearch → AzureOpenAI embeddings → SQL → chat) | `scenario14-walkthrough.gif`, `scenario14-walkthrough-long.gif` |
| 15 — Store Intelligence Report | Live | 5 | dashboard (incl. standalone `intelligence` app) → Store Intelligence app `/` (live raw signals) → refresh signals → generate report → report sections (`source: ai`) | `scenario15-walkthrough.gif`, `scenario15-walkthrough-long.gif` |
| 16 — MCP Store Operations Tools | Demo 4 (live) | 5 | dashboard → store → MCP tools selected (`SearchStoreCatalog`, `LookupProductByName`, `GetTripWeather`, `GetDestinationGuide`, `ResearchProductsOnline`) → grounded response → **Aspire traces of the MCP calls** (store → `eshopmcpserver` `/message` → products aisearch → AzureOpenAI) | `scenario16-walkthrough.gif`, `scenario16-walkthrough-long.gif` |
| 17 — A2A Store Operations Network | Demo 5 (live) | 5 | dashboard (3 agents: catalog / promotions / businessinsights) → store → A2A search `winter camping` → enriched cards (Stock / Promotions / Reviews) → **agent card** (`/.well-known/agent-card.json`) | `scenario17-walkthrough.gif`, `scenario17-walkthrough-long.gif` |
| 18 — MAF Dev UI | Demo 6 (bonus) | 2 | dashboard → store | `scenario18-walkthrough.gif`, `scenario18-walkthrough-long.gif` |

## Notes

- **Scenario 15** was re-captured after Store Intelligence was extracted into a **standalone Blazor app**
  (Aspire resource `intelligence`). The walkthrough now opens that app's home page (live raw signals +
  Refresh + Generate report) instead of the old in-Store `/intelligence` nav page.
- **Scenario 16** was re-captured after the MCP product-grid fix: broad queries
  (e.g. "what gear do I need for a rainy hike this weekend") now ground through the catalog tools and
  render the product grid, and step 5 shows the Aspire trace with the `chat` + `eshopmcpserver /message`
  spans.
- **Scenario 17** was re-captured after the A2A hardening: the `winter camping` query now returns
  enriched product cards, and each agent exposes its card at `/.well-known/agent-card.json`
  (new step 5 captures the Inventory/catalog agent card).
- **Scenario 17** required a fix: the Catalog / Promotions / BusinessInsights A2A agents crashed at
  startup on .NET 10 (`System.TypeLoadException: 'GetSwagger' ... does not have an implementation`),
  so A2A search returned 0 products. Swagger is dev-only and unused by the demo, so it was removed
  from the three agent services. A2A search now returns enriched product cards.
- **Scenario 18** is the bonus demo. `/devui` did not resolve under `aspire start` (Blazor routing can
  intercept the path), so only the dashboard + store frames were captured. See
  `demo-18scenario.md` for the DevUI reachability caveat and fallback.

## Naming convention

`scenarioNN-stepMM-<short-description>.jpg` (e.g., `scenario13-step03-run_searches.jpg`).
