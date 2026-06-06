---
name: agentic-modernization
description: Reusable session pattern for modernized .NET apps that add observability, grounded search, reporting, MCP tools, and A2A agents.
domain: architecture
confidence: medium
source: earned
tools:
  # No tool-specific patterns required yet.
---

## Context
Use this skill when a modernization story needs to move from "the app is upgraded" to "the app is now useful to agents and operators." It applies when you are planning demos or docs around observability, search grounding, store intelligence reports, safe tools, or agent collaboration.

## Patterns
- Start with the modernized app foundation: telemetry, deployability, and clear service boundaries.
- Keep the live demo local-first; treat hosted agents as an extension path.
- Reuse existing app capabilities as the source of truth for AI outputs.
- Split agent stories into focused responsibilities: observability, discovery, reporting, and orchestration.

## Examples
- Scenario 13: Observability Assistant with Foundry Local
- Scenario 16: MCP Store Operations Tools
- Scenario 17: A2A Store Operations Network

## Anti-Patterns
- Adding a generic chatbot that is not grounded in the app.
- Pitching Hosted Agents as the core demo before the local path works.
- Letting AI invent products, traces, or business signals.
