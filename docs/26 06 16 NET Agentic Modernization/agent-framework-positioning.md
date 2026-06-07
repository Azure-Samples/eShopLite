# Microsoft Agent Framework Positioning

## Purpose

This document explains how to position Microsoft Agent Framework in the session without turning the session into a framework deep dive.

## Core positioning

> Microsoft Agent Framework is the app-code layer for building agents and workflows.

In this session:

- Aspire is the app model, orchestration, observability, and deployment foundation.
- MCP exposes application capabilities as tools.
- A2A lets agents discover and collaborate with other agents.
- Microsoft Agent Framework is where we build agents and workflows in application code.
- Foundry Hosted Agents are an optional hosting/deployment path for selected agents.

## Important scope note

Do not compare agent frameworks.

Do not turn this into:

```text
Framework A vs Framework B
```

The story is:

```text
Modernized app
    ↓
Targeted AI
    ↓
App-specific agents and workflows
```

## Recommended framing

```text
Microsoft Agent Framework = build agents and workflows in app code.
MCP = expose app capabilities as safe tools.
A2A = expose agents so other agents can discover and collaborate with them.
Aspire = orchestrate, observe, and deploy the app resources.
```

## Diagram

```text
eShopLite
  ↓
App capabilities and app data
  ↓
MCP tools
  ↓
Microsoft Agent Framework agents/workflows
  ↓
A2A collaboration
  ↓
Optional Microsoft Foundry Hosted Agents
```

## When to use an agent

Use an agent when:

- The task is open-ended.
- The workflow needs reasoning.
- The agent needs to call tools.
- The user request may require planning.
- The app needs a conversational or investigative capability.

## When to use a workflow

Use a workflow when:

- The process has clear steps.
- The sequence must be controlled.
- Multiple agents/functions must coordinate.
- The app needs reliability and repeatability.
- Human-in-the-loop or checkpointing may be needed.

## Session-specific agent roles

### Catalog Agent

Responsibility:

```text
Understand product catalog data and product discovery results.
```

Uses:

- product search,
- product details,
- product comparison,
- catalog gaps.

### Observability Agent

Responsibility:

```text
Understand logs, traces, errors, and operational signals.
```

Uses:

- recent logs,
- traces,
- error groups,
- service health.

### Business Insights Agent

Responsibility:

```text
Summarize store activity and business signals.
```

Uses:

- top searches,
- failed searches,
- product opportunities,
- store intelligence report.

### Orchestrator Agent

Responsibility:

```text
Coordinate specialized agents and produce a final answer.
```

Uses:

- agent discovery,
- task breakdown,
- result aggregation,
- final recommendations.

## Suggested speaker line

> Microsoft Agent Framework is not the story by itself. The story is that once the app is modernized, we can use it to build app-specific agents and workflows that understand our data, our telemetry, and our business capabilities.

## What not to say

Avoid:

```text
This is the only way to build agents.
```

Avoid:

```text
This replaces the app.
```

Avoid:

```text
Everything should become an agent.
```

Use:

```text
Use agents where the app needs reasoning, tools, or workflow coordination.
```

## References

- Microsoft Agent Framework overview: https://learn.microsoft.com/en-us/agent-framework/overview/
- A2A integration: https://learn.microsoft.com/en-us/agent-framework/integrations/a2a
