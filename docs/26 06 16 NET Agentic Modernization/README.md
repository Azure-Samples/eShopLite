# .NET Agentic Modernization with eShopLite

This folder packages the session plan for **App Modernization Done. Now Let's Make It Smarter.**

## What is here
- Session abstract and flow
- Speaker script and demo runbook
- Demo prompts and slide outline
- Top pain points and scenario map
- Hosted Agents, Agent Framework, DevUI, and deployment notes
- References used to ground the story in Aspire

## Implementation baseline
The current repo already shows the patterns this session builds on:
- `scenarios/01-SemanticSearch/src/eShopAppHost/Program.cs`
- `scenarios/06-mcp/src/eShopAppHost/Program.cs`
- `scenarios/10-A2ANet/src/eShopAppHost/Program.cs`

Those AppHosts use Aspire composition patterns such as parameters, `WaitFor`, `WithReference`, and `IsPublishMode`-gated Azure resources.

## Working rule
This package is documentation-first. The live demo can stay on the existing scenario baselines until the new scenario scaffolds are ready.
