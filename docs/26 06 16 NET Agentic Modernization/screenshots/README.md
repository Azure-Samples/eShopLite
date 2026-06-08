# Plan-B walkthrough assets (scenarios 13–18)

Captured live via `aspire start` + Playwright (Microsoft Edge, 1600×1000) against each scenario's
running Store and Aspire Dashboard. Use these if a live demo fails — each scenario has step-by-step
JPGs plus an animated GIF (`scenarioNN-walkthrough.gif`, ~1.8 s/frame, looping).

> Note: `playwright-cli screenshot` writes PNG even when the filename ends in `.jpg`; frames here were
> converted to real JPEG with Pillow.

## Index

| Scenario | Demo | Steps | Frames | GIF |
|---|---|---:|---|---|
| 13 — Observability Assistant (Foundry Local) | Demo 2 (live) | 5 | dashboard → store → searches (+injected failure) → observability-assistant page → analysis findings (`source: foundry-local`) | `scenario13-walkthrough.gif` |
| 14 — Product Discovery Copilot | Demo 1 (code walkthrough) | 4 | dashboard → store → semantic search → grounded AI answer | `scenario14-walkthrough.gif` |
| 15 — Store Intelligence Report | Live | 5 | dashboard → `/intelligence` → refresh signals → generate report → report sections (`source: ai`) | `scenario15-walkthrough.gif` |
| 16 — MCP Store Operations Tools | Demo 4 (live) | 4 | dashboard → store → MCP tools selected (`SearchStoreCatalog`, `LookupProductByName`, `GetTripWeather`, `GetDestinationGuide`, `ResearchProductsOnline`) → grounded response | `scenario16-walkthrough.gif` |
| 17 — A2A Store Operations Network | Demo 5 (live) | 4 | dashboard (3 agents: catalog / promotions / businessinsights) → store → A2A search query → enriched cards (Stock / Promotions / Reviews) | `scenario17-walkthrough.gif` |
| 18 — MAF Dev UI | Demo 6 (bonus) | 2 | dashboard → store | `scenario18-walkthrough.gif` |

## Notes

- **Scenario 17** required a fix: the Catalog / Promotions / BusinessInsights A2A agents crashed at
  startup on .NET 10 (`System.TypeLoadException: 'GetSwagger' ... does not have an implementation`),
  so A2A search returned 0 products. Swagger is dev-only and unused by the demo, so it was removed
  from the three agent services. A2A search now returns enriched product cards.
- **Scenario 18** is the bonus demo. `/devui` did not resolve under `aspire start` (Blazor routing can
  intercept the path), so only the dashboard + store frames were captured. See
  `demo-18scenario.md` for the DevUI reachability caveat and fallback.

## Naming convention

`scenarioNN-stepMM-<short-description>.jpg` (e.g., `scenario13-step03-run_searches.jpg`).
