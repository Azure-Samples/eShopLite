# Scenario Map

## Purpose

This file maps the session story to eShopLite scenario folders, slides, demos, and docs.

## Source scenario rule

New scenarios must start from **Scenario 01 - Semantic Search** by default.

Use another existing scenario only when the new scenario specifically needs existing infrastructure, such as MCP, A2A, or deployment assets.

Each new scenario README must state:

- source scenario,
- reason for source scenario choice,
- scenario goal,
- how to run,
- demo flow,
- expected output,
- fallback plan.

## Scenario overview

| Session section | Slide | Scenario folder | Demo type | Source baseline |
|---|---:|---|---|---|
| Baseline app | 5 | Existing Scenario 01 | Live | `01 - Semantic Search` |
| Observability Assistant | 6 | `13-ObservabilityAssistantFoundryLocal` | Live | `01 - Semantic Search` |
| Product Discovery | 7 | `14-ProductDiscoveryCopilot` | Live | `01 - Semantic Search` |
| Store Intelligence | 8 | `15-StoreIntelligenceReport` | Live | `01 - Semantic Search` |
| MCP Store Tools | 9 | `16-MCPStoreOperationsTools` | Live | Prefer `01 - Semantic Search`; reuse MCP patterns if needed |
| A2A Store Network | 10 | `17-A2AStoreOperationsNetwork` | Live or architecture | Prefer `01 - Semantic Search`; reuse A2A patterns if needed |
| Hosted Agents | 11 | Docs only | Slide | Scenario 17 extension |
| Deploy with Aspire | 12 | Docs only | Slide | Existing deployment scenario/docs |
| Top 5 pain points | 13 | Docs only | Slide | Session summary |
| Final message | 14 | Docs only | Slide | Session summary |

## Scenario 13 - Observability Assistant with Foundry Local

### Goal

Make the app explain logs, traces, and recent operational activity.

### Source

`01 - Semantic Search`

### Why

The demo should feel like the same modernized app baseline, now with operational intelligence added.

### Core components

- Recent log reader or deterministic log fixture.
- Trace/correlation summary.
- Local AI summary service.
- Foundry Local integration if available.
- UI or endpoint to request summary.

### Output

Developer-friendly operational summary.

## Scenario 14 - Product Discovery Copilot

### Goal

Improve search into user-intent product discovery.

### Source

`01 - Semantic Search`

### Why

This scenario naturally extends semantic search.

### Core components

- Search input.
- Grounded product recommendation response.
- Product explanation.
- Optional comparison prompt.

### Output

Intent-based product recommendations grounded in catalog data.

## Scenario 15 - Store Intelligence Report

### Goal

Turn app signals into business summaries.

### Source

`01 - Semantic Search`

### Why

The same catalog/search baseline can produce meaningful business signals.

### Core components

- Search event fixture.
- Failed search data.
- Product catalog data.
- Optional operational summary input.
- Report generator.

### Output

Daily store intelligence report.

## Scenario 16 - MCP Store Operations Tools

### Goal

Expose app capabilities as tools.

### Source

Prefer `01 - Semantic Search`.

Reuse patterns from the existing MCP scenario only when required.

### Core tools

```text
search_catalog
get_product_details
get_failed_searches
get_store_summary
get_recent_operational_summary
generate_store_intelligence_report
```

### Output

Tool-using agent result grounded in app capabilities.

## Scenario 17 - A2A Store Operations Network

### Goal

Show specialized agents collaborating around the app.

### Source

Prefer `01 - Semantic Search`.

Reuse patterns from the existing A2A scenario or https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents when required.

### Agents

```text
Orchestrator Agent
Catalog Agent
Observability Agent
Business Insights Agent
```

### Output

Investigation summary and action plan.

## Hosted Agents evaluation

### Type

Docs and slide only unless already reliable.

### Goal

Evaluate which agents could become Microsoft Foundry Hosted Agents through Aspire.

### Output

Decision table and next-step implementation notes.

## Deploy with Aspire

### Type

Slide only.

### Goal

Show that the same modernized app model has deployment paths.

### References

- https://aspire.dev/deployment/
- https://aspire.dev/whats-new/aspire-13-4/
