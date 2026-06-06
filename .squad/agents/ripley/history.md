# Ripley History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Stack: .NET 9, .NET Aspire, Blazor, Azure OpenAI, vector search patterns
- Repo shape: scenario-driven monorepo under `scenarios/`

## Learnings
- Initial team setup completed on 2026-06-06.
- The agentic modernization story should stay app-first: telemetry and service boundaries come before MCP, A2A, and hosted-agent extensions.
- Session docs live under `docs/26 06 16 NET Agentic Modernization/`; use that folder as the canonical narrative for scenarios 13-17.
- Reuse `01-SemanticSearch`, `06-mcp`, and `10-A2ANet` as baseline patterns for discovery, tools, and agent orchestration.
