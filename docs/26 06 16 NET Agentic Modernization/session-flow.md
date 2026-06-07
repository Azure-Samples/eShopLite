# Session Flow - App Modernization Done. Now Let's Make It Smarter

## Session promise

By the end of this session, developers should understand how to add targeted AI capabilities to specific parts of a modernized .NET application without rewriting the app.

This is not a migration talk.

This is the next step after modernization.

## Core audience

- Enterprise .NET developers.
- Developers modernizing existing .NET applications.
- Architects evaluating practical AI scenarios.
- Teams already looking at Aspire, observability, cloud deployment, and agentic app patterns.

## Session positioning

The previous session may explain Aspire and modernization foundations. This session assumes that foundation exists.

This session should not become an Aspire tooling deep dive. The pitch is:

> Once the app is upgraded, how can we make it smarter?

## Timing overview

| Time | Section | Type | Main message |
|---|---|---|---|
| 00:00 - 02:00 | Opening | Slides | Modernization is the foundation, not the finish line. |
| 02:00 - 05:00 | Baseline app | Demo | eShopLite already has the modern app shape. |
| 05:00 - 10:00 | Observability Assistant | Demo | Logs and traces become explanations. |
| 10:00 - 15:00 | Product Discovery | Demo | Search becomes intent-based discovery. |
| 15:00 - 19:00 | Store Intelligence | Demo | App signals become business summaries. |
| 19:00 - 23:00 | MCP Store Tools | Demo | App capabilities become agent tools. |
| 23:00 - 27:00 | A2A Agent Workflow | Demo / architecture | Specialized agents collaborate around the app. |
| 27:00 - 29:00 | Deploy with Aspire | Slides | The same app model has deployment options. |
| 29:00 - 30:00 | Closing | Slides | Modernization makes practical AI possible. |

## Flow diagram

```text
Modernized eShopLite
    ↓
Telemetry + logs + traces
    ↓
Observability Assistant
    ↓
Product Discovery
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

## 1. Opening - 00:00 to 02:00

### Goal

Set the premise quickly.

### Speaker line

> Most modernization talks stop when the app runs on the new stack. Today I want to start there. The app is already modernized. Now what?

### Message

Modernization gives us:

- newer runtime,
- better app structure,
- telemetry,
- deployment paths,
- service boundaries,
- better developer experience.

But that is not the final value.

### Audience takeaway

> Modernization is not the end state. It creates the foundation for smarter apps.

### Transition

> If the app already has structure, signals, and boundaries, then AI has something useful to work with.

## 2. Baseline app - 02:00 to 05:00

### Goal

Show eShopLite as the modernized foundation.

### Show

- eShopLite running locally.
- Aspire dashboard.
- App resources.
- Logs/traces.
- Product catalog.
- Existing search experience.
- AppHost structure if useful.

### Speaker line

> This is not a rewrite story. This is an incremental improvement story.

### What to avoid

Do not spend too much time explaining Aspire internals. The previous session covers Aspire. Here, Aspire is the foundation.

### Audience takeaway

> Because the app is modernized, we can now add AI in specific places instead of bolting on a generic chatbot.

### Transition

> The first place modernization helps is operations. We already have logs and traces. Can the app explain itself?

## 3. Demo 1 - Observability Assistant - 05:00 to 10:00

### Scenario

`13-ObservabilityAssistantFoundryLocal`

### Goal

Show that logs and traces can become an explanation.

### Setup

- Use eShopLite baseline from Scenario 01.
- Generate search/cart/catalog activity.
- Trigger or simulate one or two known issues.
- Show logs/traces in the local developer environment.
- Use a local AI service, ideally Foundry Local, to summarize recent operational signals.

### Demo flow

1. Open eShopLite.
2. Perform a few user actions.
3. Trigger a failed search or catalog issue.
4. Show raw logs/traces.
5. Ask the Observability Assistant to summarize.
6. Show grouped issues, root cause candidates, and next actions.

### Demo prompt

```text
Summarize the last 10 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

### Expected output shape

```text
Summary
- What happened
- Services involved
- Most likely root cause
- Supporting log/trace signals
- Recommended next actions
```

### Speaker line

> This is where modernization pays off immediately. We already have telemetry. Now we use AI to make that telemetry easier to understand.

### Audience takeaway

> AI is useful here because modernization already gave us telemetry.

### Fallback

If the AI service fails:

- show a saved output sample,
- explain the expected input/output,
- show the raw logs and how the summary would help,
- move to Product Discovery.

### Transition

> The app can now help developers understand itself. But users also need smarter experiences.

## 4. Demo 2 - Product Discovery - 10:00 to 15:00

### Scenario

`14-ProductDiscoveryCopilot`

### Goal

Show how search becomes product discovery.

### Setup

- Start from Scenario 01.
- Use the existing catalog and search experience.
- Add an AI-assisted product discovery layer.
- Keep answers grounded in product/catalog data.

### Demo flow

1. Show normal keyword search.
2. Search with a vague user-intent query.
3. Show semantic/product-discovery results.
4. Ask the assistant to explain why products match.
5. Show grounded responses that cite product data.

### Demo prompts

```text
Find products that are good for walking all day and explain why each result matches.
```

```text
Show me products under $100 that would be useful for a summer trip.
```

```text
Compare these three products and recommend the best one for daily commuting.
```

### Speaker line

> This is not a random chatbot. This improves a feature every user already understands: search.

### Audience takeaway

> Users do not search like databases think. AI helps the app understand intent, not only keywords.

### Fallback

If semantic results are unstable:

- use deterministic product data,
- show a prepared answer,
- explain the grounding strategy,
- avoid making claims that are not in the product catalog.

### Transition

> Now we have smarter operations and smarter UX. What about the business users who need to understand what is happening in the app?

## 5. Demo 3 - Store Intelligence Report - 15:00 to 19:00

### Scenario

`15-StoreIntelligenceReport`

### Goal

Turn app-generated signals into a business summary.

### Setup

- Use product catalog, search events, failed searches, cart events, and optional operational issues.
- Provide deterministic sample data so the demo is reliable.

### Demo flow

1. Show recent store activity.
2. Show raw events or simple tables.
3. Generate a daily store intelligence report.
4. Highlight failed searches, product opportunities, trends, and operational issues.
5. Show recommended next actions.

### Demo prompt

```text
Create today's store intelligence report. Include top searches, failed searches, product opportunities, operational issues that may affect customers, and recommended next actions.
```

### Expected output shape

```text
Store Intelligence Report
- Executive summary
- Top customer intents
- Searches with no results
- Product gaps
- Operational issues
- Recommended actions
```

### Speaker line

> Apps already collect signals. Business users need summaries, patterns, and next actions.

### Audience takeaway

> Modern apps generate signals. Smarter apps turn those signals into decisions.

### Fallback

If live generation is too slow:

- show a saved report,
- explain which app signals were used,
- show the prompt and expected schema.

### Transition

> So far, we used AI inside the app. The next step is exposing app capabilities so agents can use them safely.

## 6. Demo 4 - MCP Store Tools - 19:00 to 23:00

### Scenario

`16-MCPStoreOperationsTools`

### Goal

Show app capabilities exposed as tools.

### Positioning

MCP matters here because the modernized app has clean capabilities:

- search catalog,
- get product details,
- get failed searches,
- get store summary,
- get recent operational summary.

This is not a generic MCP session. It is an application modernization story.

### Demo flow

1. Show the app capability list.
2. Show MCP tools mapped to real app capabilities.
3. Run a prompt that uses multiple tools.
4. Show the answer grounded in app data.
5. Explain how this avoids a generic chatbot.

### Demo prompt

```text
Customers are searching for travel products. Use the store tools to find matching products, identify failed searches, and summarize product gaps.
```

### Speaker line

> MCP is not the demo because MCP is cool. MCP matters because our modernized app now has clean capabilities that an agent can safely use.

### Audience takeaway

> MCP turns app capabilities into agent-ready tools.

### Fallback

If tool calling is not ready:

- show the tool contract,
- show sample request/response,
- show the same scenario as an architecture walkthrough.

### Transition

> MCP lets one agent use the app. But real enterprise workflows often need more than one agent.

## 7. Demo 5 - A2A Store Operations Network - 23:00 to 27:00

### Scenario

`17-A2AStoreOperationsNetwork`

### Goal

Show specialized agents collaborating around the app.

### Agent roles

```text
Orchestrator Agent
  ├── Catalog Agent
  ├── Observability Agent
  └── Business Insights Agent
```

Optional extension:

```text
NeMo-style Analysis Agent
Microsoft Agent Framework Action Agent
Foundry Hosted Agent candidate
```

### Demo prompt

```text
Customers are searching for travel products but not converting. Investigate the likely reasons and suggest actions.
```

### Demo flow

1. User asks an investigation question.
2. Orchestrator Agent breaks down the task.
3. Catalog Agent checks products.
4. Business Insights Agent checks searches/conversions.
5. Observability Agent checks errors/latency.
6. Orchestrator Agent returns recommendations.

### Speaker line

> MCP lets an agent use tools. A2A lets agents collaborate. This is closer to real enterprise workflows.

### Hosted Agents note

> This is also where we evaluate the next deployment shape: should these agents stay local for the demo, or should some become Microsoft Foundry Hosted Agents managed through Aspire? For this session, the main point is the smarter workflow, not the hosting mechanism.

### Audience takeaway

> Real enterprise workflows usually need specialized agents, not one giant chatbot.

### Fallback

If A2A is not ready live:

- show architecture diagram,
- show agent cards,
- show sample messages,
- show expected final output.

### Transition

> We added intelligence around operations, users, business signals, tools, and agents. The final question is: how do we move this forward?

## 8. Deploy with Aspire - 27:00 to 29:00

### Type

Slide-only.

### Goal

Explain that the same app model has a deployment path.

### Message

> Deployment is not the demo today, but it matters.

### Talk track

- Aspire gives us a consistent app model.
- `aspire publish` can emit deployment artifacts.
- `aspire deploy` can resolve parameters and apply changes.
- The same local app topology can move toward Azure Container Apps, Azure App Service, Kubernetes, Docker Compose, or custom pipelines.
- The previous Aspire session can go deeper into these flows.

### Audience takeaway

> Aspire keeps the modernization foundation portable.

### Transition

> Now let's close with the pain points this pattern addresses.

## 9. Closing - 29:00 to 30:00

### Final slide message

```text
Modernization is not the finish line.

It is the foundation that lets us make apps smarter.
```

### Top 5 pain points

1. We have logs, but nobody has time to read them.
2. Users search with intent, not database keywords.
3. Apps collect signals but do not explain them.
4. Integrations still require custom glue.
5. Real workflows need multiple specialized agents.

### Closing line

> The best time to add useful AI to an app is after the app has been modernized. Now the app has the structure, telemetry, data, and boundaries that AI needs.
