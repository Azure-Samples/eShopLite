# .NET Agentic Modernization Session

## Session title

**App Modernization Done. Now Let's Make It Smarter**

## Session promise

This session starts where most modernization sessions stop.

The app has already been upgraded. It runs on modern .NET. It uses Aspire. It has logs, traces, metrics, service boundaries, deployment options, and real application data.

Now the question is:

> What else can we do to make this application smarter?

This folder is the speaker and demo kit for the Microsoft Reactor session.

Event URL:

https://developer.microsoft.com/en-us/reactor/events/27243/

Repository:

https://github.com/Azure-Samples/eShopLite

## Core message

> Modernization is not the finish line. It is the foundation that lets us make our apps smarter.

Use this sharper version throughout the session:

> We already upgraded the app. Now the app can explain itself, understand users better, summarize business signals, expose capabilities as tools, and collaborate through agents.

## What is in scope

- AI over logs, traces, metrics, and operational signals.
- AI over product search and user experience.
- AI over business activity and app-generated data.
- MCP as a way to expose app capabilities as tools.
- A2A as a way for specialized agents to collaborate.
- Microsoft Agent Framework as the app-code layer for agents and workflows.
- DevUI as an optional development/testing aid for agents.
- Foundry Hosted Agents as an evaluation path for agent scenarios.
- Aspire deployment as a slide-only section.

## What is out of scope

- Smart Components.
- eShopSupport.
- eShopOnWeb.
- Aspire MCP server / AI coding-agent workflow as a session pitch.
- A deep dive into Aspire tooling.
- A live deployment demo.
- DevUI as a production feature.

The Aspire MCP / AI coding-agent story belongs to the dedicated Aspire session. This session uses Aspire as the modernization foundation, then focuses on making the upgraded app smarter.

## Recommended reading order

1. `session-flow.md`
2. `slide-content.md`
3. `scenario-map.md`
4. `demo-runbook.md`
5. `demo-prompts.md`
6. `hosted-agents-evaluation.md`
7. `agent-framework-positioning.md`
8. `devui-agent-validation-notes.md`
9. `deploy-with-aspire-slide-notes.md`
10. `references.md`

## Scenario list

The new scenarios should be added as new folders under `/scenarios`.

New scenarios must start from **Scenario 01 - Semantic Search** by default, unless a scenario specifically requires infrastructure from another scenario.

| Scenario | Name | Source baseline | Session section |
|---|---|---|---|
| 13 | `13-ObservabilityAssistantFoundryLocal` | `01 - Semantic Search` | Smarter operations |
| 14 | `14-ProductDiscoveryCopilot` | `01 - Semantic Search` | Smarter user experience |
| 15 | `15-StoreIntelligenceReport` | `01 - Semantic Search` | Smarter business operations |
| 16 | `16-MCPStoreOperationsTools` | Prefer `01 - Semantic Search`; reuse MCP patterns if needed | Smarter integrations |
| 17 | `17-A2AStoreOperationsNetwork` | Prefer `01 - Semantic Search`; reuse A2A patterns if needed | Smarter workflows |

## Branch rule

All work should happen in this branch:

```bash
git checkout -b bruno-NETAgenticModernizationSession
```

## Folder purpose

This folder is not only implementation documentation. It is the full session kit:

- speaker story,
- slide content,
- demo sequence,
- prompts,
- scenario map,
- Hosted Agents evaluation,
- Agent Framework positioning,
- DevUI notes,
- deployment slide notes,
- references.

## Main narrative

```text
We have a modernized app.
It runs with Aspire.
It has telemetry.
It has real business data.
It has clean service boundaries.
It can be deployed.

Now we make it smarter.
```

## Closing message

> The best time to add useful AI to an app is after modernization, because the app now has the structure, telemetry, data, and boundaries that AI needs.
