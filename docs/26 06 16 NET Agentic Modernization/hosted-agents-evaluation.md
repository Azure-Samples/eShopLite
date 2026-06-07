# Hosted Agents Evaluation

## Purpose

This document evaluates whether the agent-based scenarios in the session should stay local, be exposed as Microsoft Foundry Hosted Agents, or support both paths.

This is an evaluation and extension path. It is not a live-demo requirement.

## Session positioning

The live session should focus on:

> How do we make an upgraded .NET app smarter?

Hosted Agents should be framed as:

> Once the app has agent-ready capabilities, which agents could become deployable resources?

Do not turn this into a deployment deep dive.

## Official docs anchor

Aspire 13.4 describes the ability to turn project or app resources into Microsoft Foundry Hosted Agents, interact with them through the Aspire dashboard, and deploy them with Aspire.

Reference:

https://aspire.dev/whats-new/aspire-13-4/#foundry-hosted-agent-commands-and-fixes

## Key question

For each agent:

```text
Should this agent stay local, become a Hosted Agent, or support both?
```

## Agent decision table

| Agent | Keep local for live demo | Hosted Agent candidate | Reason |
|---|---:|---:|---|
| Catalog Agent | Yes | Maybe | Needs direct app/catalog context. Could become hosted if catalog access is exposed safely. |
| Observability Agent | Yes | Maybe | Useful near logs/traces. Hosting depends on telemetry access and data boundaries. |
| Business Insights Agent | Yes | Yes | Good candidate for recurring summaries and business reports. |
| Orchestrator Agent | Yes | Yes | Could coordinate local and hosted agents. Needs reliable identity/configuration. |
| NeMo-style Analysis Agent | Maybe | Maybe | Depends on runtime, containerization, model dependencies, and hosting constraints. |
| Recommendation Agent | Yes | Yes | Good candidate if product data access is clean and grounded. |

## Recommended decision for this session

For the live session:

```text
Keep the A2A demo local.
Present Hosted Agents as the cloud-ready extension path.
```

Rationale:

- It keeps the session focused.
- It avoids relying on live cloud deployment.
- It lets the audience understand the architecture before hosting details.
- It preserves time for the main message: smarter modernized apps.

## Candidate Hosted Agent: Business Insights Agent

### Why it is a good candidate

- Business reports can be generated on demand or on schedule.
- It has a clear responsibility.
- It can use controlled inputs.
- It produces a bounded output.
- It does not need to mutate production systems.

### Required capabilities

- Access to store activity data.
- Access to product catalog summaries.
- Access to failed search data.
- Optional access to operational summaries.
- Secure identity for data access.

### Risks

- It may invent business metrics if not grounded.
- It may need strong output schema validation.
- It may need clear data-retention boundaries.

## Candidate Hosted Agent: Orchestrator Agent

### Why it is a good candidate

- It coordinates specialized agents.
- It can route requests to local or hosted agents.
- It can preserve a stable workflow API.

### Required capabilities

- Agent discovery.
- A2A endpoint configuration.
- Identity/authentication.
- Retry/fallback handling.
- Response aggregation.

### Risks

- It can become too generic.
- It may hide failures from specialized agents.
- It needs clear traceability.

## Candidate Hosted Agent: Observability Agent

### Why it may be useful

- It can explain app health.
- It can summarize recent incidents.
- It can support support/operations workflows.

### Why it may stay local

- Operational telemetry may be sensitive.
- Local demo is simpler and safer.
- Access to logs/traces may differ by environment.

## Required evaluation output

Create this file for Scenario 17:

```text
/scenarios/17-A2AStoreOperationsNetwork/docs/foundry-hosted-agent-evaluation.md
```

It should include:

1. Agent inventory.
2. Local vs Hosted decision.
3. Required protocols.
4. Required configuration.
5. Identity and permissions.
6. Data access assumptions.
7. Security notes.
8. Demo-readiness status.
9. Future implementation steps.

## Aspire API verification note

Aspire 13.4 includes Hosted Agent changes and breaking-change notes. During implementation, verify the exact API names in the current package version used by the repository.

Do not blindly copy preview API names.

Implementation must check current docs/API for:

- hosted-agent modeling,
- compute environment methods,
- prompt agent APIs,
- identity behavior,
- environment variables,
- deployment behavior.

## Suggested slide message

```text
The live demo can stay local.
The architecture can still be cloud-ready.
Hosted Agents are the next deployment shape for selected agents.
```

## Suggested speaker note

> The important idea is not that every agent must be hosted. The important idea is that modernization gave us the app model, data boundaries, and app capabilities needed to decide where each agent belongs.
