# Scenario Map

## Purpose

Map the session story to real scenario folders, with a reliability-first view for local demos.

## Session guardrails

- Keep the session scope from `docs\26 06 16 NET Agentic Modernization\README.md`.
- Keep out-of-scope items unchanged (no Smart Components, no eShopSupport/eShopOnWeb, no live deployment demo, no Aspire MCP server pitch).
- Prioritize runnable local demos over speculative architecture changes.

## Source scenario rule

New scenarios should start from **01-SemanticSearch** unless a scenario explicitly needs existing infrastructure from another scenario.

For this session package, reliability comes first:
- Scenario 16 currently depends on MCP infrastructure from Scenario 06.
- Scenario 17 currently depends on A2A infrastructure from Scenario 10.

## Current scenario reality (2026-06)

| Session section | Slide | Scenario folder | Current code baseline | Current readiness | Demo type now |
|---|---:|---|---|---|---|
| Baseline app | 5 | Existing Scenario 01 | `01-SemanticSearch` | Runnable | Live |
| Product Discovery | 6 | `14-ProductDiscoveryCopilot` | `01-SemanticSearch` (copied) | Runnable baseline; presented as code walkthrough this session | Code walkthrough (no run) |
| Observability Assistant | 7 | `13-ObservabilityAssistantFoundryLocal` | Local runnable, modernization-first flow | Demo narrative uses three Aspire services (`products`, `store`, `observabilityassistant`) with backend findings shown in Store; analysis runs locally with Foundry Local | Live (local AI) |
| Store Intelligence | 8 | `15-StoreIntelligenceReport` | `01-SemanticSearch` (copied) | Implemented: signal capture + report endpoint + `/intelligence` page (deterministic fallback) | Live (Generate report; `source: ai\|fallback`) |
| MCP Store Tools | 9 | `16-MCPStoreOperationsTools` | `06-mcp` | Runnable MCP sample; store-ops tool set not fully aligned to session script | Live (existing MCP tools) |
| A2A Store Network | 10 | `17-A2AStoreOperationsNetwork` | `10-A2ANet` | Runnable A2A sample; agent roles differ from session script | Live (existing agent network) |
| Hosted Agents | 11 | Docs only | N/A | Design/evaluation only | Slide |
| Deploy with Aspire | 12 | Docs only | N/A | Design only for this session | Slide |
| Top 5 pain points | 13 | Docs only | N/A | Ready | Slide |
| Final message | 14 | Docs only | N/A | Ready | Slide |

## Scenario targets and known gaps

## Scenario 13 - Observability Assistant with Foundry Local

- **Session target:** Operational-summary assistant over logs/traces.
- **Demo position:** **Demo 2** (after Product Discovery) — the "local AI" beat: the analysis model runs on the machine via Foundry Local.
- **Demo flow:** Store requests analysis from `observabilityassistant`; assistant summarizes telemetry and Store renders findings.
- **Window flow:** 5 / 10 / 15 / 30-minute analysis windows in one local run.

## Scenario 14 - Product Discovery Copilot

- **Session target:** Intent-first discovery and grounded explanations.
- **Demo position:** **Demo 1** (opens the demo arc) — the "better answers for users with cloud AI" beat.
- **Demo format:** **Code walkthrough — no app run.** Read
  `src/Products/Memory/MemoryContext.cs` and explain how data is grounded (startup embedding
  of the catalog, query embedding, `Score > 0.3` gate, grounded prompt built only from matched
  products). Full script: `demo-14scenario.md`.
- **Current:** Same app code as Scenario 01; the discovery + grounding logic lives in
  `MemoryContext.cs`.
- **Why walkthrough:** the grounding technique is the teachable moment; showing it in code is
  clearer and more reliable than a live search for this session.

## Scenario 15 - Store Intelligence Report

- **Session target:** Daily store-intelligence summary from app signals.
- **Current:** Implemented end-to-end. Search endpoints record `StoreSignal`s into a seeded
  `StoreSignalStore` on `products`; `StoreIntelligenceReportService` aggregates them and writes the
  summary via `IChatClient` with a deterministic fallback; `/api/intelligence/{signals,report}`
  expose them; the `store` **Store Intelligence** page (`/intelligence`) renders the report with a
  `source: ai|fallback` badge. Verified live (`source: ai`, 8 signals, failed searches surfaced).
- **Gap to target:** None for the demo. Optional later: persist signals to SQL; add cart/checkout
  event signals.

## Scenario 16 - MCP Store Operations Tools

- **Session target toolset:** store operations and intelligence tools.
- **Current runnable toolset:** product search + weather + park info + online research in `src\eShopMcpSseServer\Tools`.
- **Gap to target:** add/rename tools to match store-operations narrative and demo prompts.

## Scenario 17 - A2A Store Operations Network

- **Session target roles:** Orchestrator + Catalog + Observability + Business Insights agents.
- **Current runnable roles:** Inventory + Promotions + Researcher agents orchestrated by Products API.
- **Gap to target:** role remap and prompt alignment while preserving runnable A2A flow.

## Demo reliability policy for this session

1. Use runnable local paths first.
2. Keep deterministic fallback outputs for every scenario.
3. Do not block the session on cloud-only dependencies.
4. Treat Hosted Agents as evaluation content unless proven stable for live demo.
