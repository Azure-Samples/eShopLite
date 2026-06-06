# Newt History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Role: Aspire Expert

## Source of Truth
- `https://aspire.dev/`
- `https://aspire.dev/get-started/install-cli/`
- `https://aspire.dev/get-started/app-host/`
- `https://aspire.dev/get-started/ai-coding-agents/`

## Learnings
- This agent is constrained to `aspire.dev` as the only allowed external source domain.

- The modernization session is being packaged as a documentation-first bundle under `docs/26 06 16 NET Agentic Modernization/` with a README, abstract, flow, runbook, prompts, slide notes, and evaluation pages.
- Existing AppHost baselines already demonstrate the composition patterns the session needs: `AddParameter`, `WaitFor`, `WithReference`, and `ExecutionContext.IsPublishMode` in `scenarios/01-SemanticSearch/src/eShopAppHost/Program.cs`, plus MCP and A2A baselines in `scenarios/06-mcp/src/eShopAppHost/Program.cs` and `scenarios/10-A2ANet/src/eShopAppHost/Program.cs`.
- The live demo can stay grounded in the existing 01/06/10 scenarios while the planned 13-17 story labels remain doc-level targets.
- Scenario scaffolds for 13-17 are present as README-first placeholders under `scenarios/13-17-*`; they link back to the shared session package and should be treated as next-step implementation targets.
