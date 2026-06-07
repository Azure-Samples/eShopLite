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
| Observability Assistant | 6 | `13-ObservabilityAssistantFoundryLocal` | `01-SemanticSearch` (copied) | Runnable baseline; feature-specific assistant not implemented | Live (baseline + scripted output) |
| Product Discovery | 7 | `14-ProductDiscoveryCopilot` | `01-SemanticSearch` (copied) | Runnable baseline; Copilot UX not implemented | Live (baseline semantic search) |
| Store Intelligence | 8 | `15-StoreIntelligenceReport` | `01-SemanticSearch` (copied) | Runnable baseline; report pipeline not implemented | Live (prepared report fallback) |
| MCP Store Tools | 9 | `16-MCPStoreOperationsTools` | `06-mcp` | Runnable MCP sample; store-ops tool set not fully aligned to session script | Live (existing MCP tools) |
| A2A Store Network | 10 | `17-A2AStoreOperationsNetwork` | `10-A2ANet` | Runnable A2A sample; agent roles differ from session script | Live (existing agent network) |
| Hosted Agents | 11 | Docs only | N/A | Design/evaluation only | Slide |
| Deploy with Aspire | 12 | Docs only | N/A | Design only for this session | Slide |
| Top 5 pain points | 13 | Docs only | N/A | Ready | Slide |
| Final message | 14 | Docs only | N/A | Ready | Slide |

## Scenario targets and known gaps

## Scenario 13 - Observability Assistant with Foundry Local

- **Session target:** Operational-summary assistant over logs/traces.
- **Current:** Same app code as Scenario 01; scenario-specific docs are still mostly Semantic Search copy.
- **Gap to target:** Add observability assistant flow (or deterministic fixture-backed endpoint) and aligned docs.

## Scenario 14 - Product Discovery Copilot

- **Session target:** Intent-first discovery and grounded explanations.
- **Current:** Same app code as Scenario 01; no dedicated Copilot flow.
- **Gap to target:** Add lightweight discovery/copilot interaction layer and scenario-specific docs.

## Scenario 15 - Store Intelligence Report

- **Session target:** Daily store-intelligence summary from app signals.
- **Current:** Same app code as Scenario 01; no implemented report pipeline/page.
- **Gap to target:** Add deterministic input fixture + report generation endpoint/UI + docs.

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
