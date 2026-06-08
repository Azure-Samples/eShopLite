# Copilot Handoff Context - .NET Agentic Modernization Session

## Purpose

This file transfers the working context from ChatGPT to GitHub Copilot so Copilot can continue refining the session content, slide story, scenario documentation, and implementation plan inside the eShopLite repository.

## Repository

```text
https://github.com/Azure-Samples/eShopLite
```

Local repo path used by Bruno:

```text
d:\azure-samples\eShopLite
```

## Branch

All work must happen in:

```text
bruno-NETAgenticModernizationSession
```

If the branch does not exist, create it from `main`.

```powershell
git checkout main
git pull origin main
git checkout -b bruno-NETAgenticModernizationSession
```

If it already exists:

```powershell
git checkout bruno-NETAgenticModernizationSession
```

## Session docs folder

All session docs are under:

```text
docs/26 06 16 NET Agentic Modernization/
```

## Current status

The folder has already been replaced with the new generated session docs. The new files appear to be working fine.

Copilot should now review and refine the content, not start from scratch.

## Main session title

```text
App Modernization Done. Now Let's Make It Smarter
```

## Main session promise

The app has already been modernized.

It runs on modern .NET. It uses Aspire. It has logs, traces, metrics, deployment options, service boundaries, and real application data.

Now the session asks:

```text
What else can we do to make this app smarter?
```

## Core message

```text
Modernization is not the finish line. It is the foundation that lets us make our apps smarter.
```

Use this sharper version across the session:

```text
We already upgraded the app. Now the app can explain itself, understand users better, summarize business signals, expose capabilities as tools, and collaborate through agents.
```

## Important scope rules

### In scope

- AI over logs, traces, metrics, and operational signals.
- AI over product search and user experience.
- AI over business activity and app-generated data.
- MCP as a way to expose app capabilities as tools.
- A2A as a way for specialized agents to collaborate.
- Microsoft Agent Framework as the app-code layer for agents and workflows.
- DevUI as an optional development/testing aid for agents.
- Foundry Hosted Agents as an evaluation path for agent scenarios.
- Aspire deployment as a slide-only section.

### Out of scope

- Smart Components.
- eShopSupport.
- eShopOnWeb.
- Aspire MCP server / AI coding-agent workflow as a session pitch.
- Deep dive into Aspire tooling.
- Live deployment demo.
- DevUI as a production feature.
- Generic chatbot-first pitch.

## Why Aspire MCP / AI coding agents are out of scope

There is another Aspire-focused session before this one. This session should not repeat the Aspire tooling story.

This session uses Aspire as the modernization foundation, then focuses on:

```text
How do we make the upgraded app smarter?
```

Do not add Aspire MCP server / AI coding-agent content to the main slides, session flow, or pitch.

## Demo story arc

```text
Modernized eShopLite
    ↓
Telemetry + logs + traces
    ↓
Product Discovery
    ↓
Observability Assistant
    ↓
Store Intelligence
    ↓
MCP Store Tools
    ↓
A2A Store Operations Network
    ↓
Hosted Agents evaluation
    ↓
Deploy with Aspire
```

## Proposed session sections

| Time | Section | Type | Main message |
|---|---|---|---|
| 00:00 - 02:00 | Opening | Slides | Modernization is the foundation, not the finish line. |
| 02:00 - 05:00 | Baseline app | Demo | eShopLite already has the modern app shape. |
| 05:00 - 10:00 | Product Discovery | Code walkthrough (no run) | Search becomes intent-based discovery. |
| 10:00 - 15:00 | Observability Assistant | Demo (local AI) | Logs and traces become explanations with Foundry Local. |
| 15:00 - 19:00 | Store Intelligence | Demo | App signals become business summaries. |
| 19:00 - 23:00 | MCP Store Tools | Demo | App capabilities become agent tools. |
| 23:00 - 27:00 | A2A Agent Workflow | Demo / architecture | Specialized agents collaborate around the app. |
| 27:00 - 29:00 | Deploy with Aspire | Slides | The same app model has deployment options. |
| 29:00 - 30:00 | Closing | Slides | Modernization makes practical AI possible. |

## New scenario folders

The expected new scenarios are:

```text
scenarios/13-ObservabilityAssistantFoundryLocal
scenarios/14-ProductDiscoveryCopilot
scenarios/15-StoreIntelligenceReport
scenarios/16-MCPStoreOperationsTools
scenarios/17-A2AStoreOperationsNetwork
```

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

## Scenario mapping

| Scenario | Name | Source baseline | Session section |
|---|---|---|---|
| 13 | `13-ObservabilityAssistantFoundryLocal` | `01 - Semantic Search` | Smarter operations |
| 14 | `14-ProductDiscoveryCopilot` | `01 - Semantic Search` | Smarter user experience |
| 15 | `15-StoreIntelligenceReport` | `01 - Semantic Search` | Smarter business operations |
| 16 | `16-MCPStoreOperationsTools` | Prefer `01 - Semantic Search`; reuse MCP patterns if needed | Smarter integrations |
| 17 | `17-A2AStoreOperationsNetwork` | Prefer `01 - Semantic Search`; reuse A2A patterns if needed | Smarter workflows |

## Main files Copilot should review first

```text
docs/26 06 16 NET Agentic Modernization/session-flow.md
docs/26 06 16 NET Agentic Modernization/slide-content.md
docs/26 06 16 NET Agentic Modernization/scenario-map.md
docs/26 06 16 NET Agentic Modernization/demo-runbook.md
docs/26 06 16 NET Agentic Modernization/demo-prompts.md
```

Then review:

```text
docs/26 06 16 NET Agentic Modernization/hosted-agents-evaluation.md
docs/26 06 16 NET Agentic Modernization/agent-framework-positioning.md
docs/26 06 16 NET Agentic Modernization/devui-agent-validation-notes.md
docs/26 06 16 NET Agentic Modernization/deploy-with-aspire-slide-notes.md
docs/26 06 16 NET Agentic Modernization/top-5-pain-points.md
docs/26 06 16 NET Agentic Modernization/references.md
```

## Desired output quality

The docs should feel like a speaker kit, not generated placeholders.

Every main file should help Bruno rehearse and deliver the session.

The most important improvements are:

1. More detailed session flow.
2. More complete slide content.
3. Better transitions.
4. More realistic speaker notes.
5. Clear demo goals.
6. Clear fallback plans.
7. Stronger link between scenarios and the session story.
8. Consistent use of the core message.

## Tone and style

Use Bruno's technical community style:

- practical,
- clear,
- enthusiastic,
- developer-first,
- not overhyped,
- friendly,
- enterprise-relevant.

Avoid overly formal marketing language.

Use short punchy lines for slides.

Use more detailed guidance in speaker notes.

## Do not invent

Do not invent product capabilities, repo content, APIs, or cloud behavior.

When a feature is only an evaluation or future path, label it clearly.

Especially:

- Hosted Agents are an evaluation path unless already implemented and reliable.
- DevUI is optional and development-only.
- Deploy with Aspire is slide-only.
- Aspire MCP / AI coding-agent pitch is out of scope.

## References

- eShopLite: https://github.com/Azure-Samples/eShopLite
- Aspire 13.4: https://aspire.dev/whats-new/aspire-13-4/
- Aspire Hosted Agents: https://aspire.dev/whats-new/aspire-13-4/#foundry-hosted-agent-commands-and-fixes
- Aspire deployment: https://aspire.dev/deployment/
- Microsoft Agent Framework: https://learn.microsoft.com/en-us/agent-framework/overview/
- Microsoft Agent Framework A2A: https://learn.microsoft.com/en-us/agent-framework/integrations/a2a
- Microsoft Agent Framework DevUI: https://learn.microsoft.com/en-us/agent-framework/devui/
- MAF + A2A + NVIDIA NeMo sample: https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents
