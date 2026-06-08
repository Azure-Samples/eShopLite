# MAF Dev UI

**Status:** Bonus demo. Hosts Microsoft Agent Framework (MAF) agents inside the Store and exposes the Agent Framework Dev UI at `/devui` (Development only). Builds clean (`eShopLite-18-MAFDevUI.slnx`). Demo script: `docs/26 06 16 NET Agentic Modernization/demo-18scenario.md`. Plan-B walkthrough assets: `docs/26 06 16 NET Agentic Modernization/screenshots/scenario18-*`.

> ⚠️ This scenario's AppHost is at the **scenario root** (`18-MAFDevUI/eShopAppHost`), not under a `src/` folder. Start from there with `aspire start`.

> ⚠️ `/devui` may return 404 under `aspire start` (Blazor routing can intercept the path). If so, run the Store standalone with `dotnet run` (Development) from `18-MAFDevUI/Store`, and confirm Foundry-hosted agents are configured. See the demo script for the full caveat and fallback.

## Derived from
14 - MAF Dev UI (the original scenario 14, renumbered to 18 so `14-ProductDiscoveryCopilot` could take the session's Demo 1 slot).

## What this scenario demonstrates
- Microsoft Agent Framework (MAF) agents hosted inside the Store.
- The Agent Framework Dev UI (`/devui`) for inspecting and running agents in the browser.
- An agent-driven checkout workflow (`AgentServices`).
