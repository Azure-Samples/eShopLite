# Slide Content - App Modernization Done. Now Let's Make It Smarter

## Deck guidance

This file contains slide-ready content: slide titles, bullets, suggested visuals, speaker notes, demo prompts, transitions, and fallback notes.

The tone should be practical, enterprise-friendly, and developer-first.

The deck should not feel like an AI hype talk. It should feel like:

> We upgraded the app. Here are practical ways to make it smarter.

## Slide 1 - Title

### Title

**App Modernization Done. Now Let's Make It Smarter**

### Subtitle

Adding targeted AI to modern .NET applications with eShopLite, Aspire, Microsoft Foundry, MCP, A2A, and Microsoft Agent Framework.

### Visual idea

Simple layered diagram:

```text
Modernized .NET app
    ↓
Telemetry + data + APIs
    ↓
Targeted AI capabilities
    ↓
Smarter app
```

### Speaker notes

Most modernization talks focus on getting the app onto a newer stack. That is important, but today I want to start right after that moment.

The app is already modernized.

Now what can we do to make it smarter?

### Transition

Let's start with what the upgraded app gives us.

---

## Slide 2 - The app is upgraded. Now what?

### Main message

Modernization gives us the foundation for practical AI.

### Bullets

- The app runs on modern .NET.
- Services are orchestrated with Aspire.
- Logs, traces, and metrics are available.
- Deployment options are clearer.
- The app has cleaner boundaries.
- The app has real data and real user workflows.

### Punchline

**Now AI has something useful to work with.**

### Speaker notes

A modernized app is easier to observe, easier to deploy, easier to reason about, and easier to extend.

That is exactly what AI needs. AI is not magic dust that fixes a messy app. It works best when the app has structure, signals, and clear capabilities.

### Transition

So the question is not "how do we add a chatbot?" The question is "where can AI improve the app?"

---

## Slide 3 - The modernization foundation

### Main message

The upgraded app gives AI useful inputs and safe boundaries.

### Visual

```text
Modern .NET
+ Aspire
+ OpenTelemetry
+ Clean APIs
+ App data
+ Search layer
+ Deployment model
= AI-ready application foundation
```

### Bullets

- Telemetry gives AI operational context.
- Data gives AI grounding.
- APIs give AI safe capabilities.
- AppHost gives AI and developers a clear app topology.
- Deployment flows give us a path forward.

### Speaker notes

This is why we are starting from eShopLite. It is not a toy chatbot demo. It is a modern .NET application where we can add intelligence to specific parts of the app.

### Transition

Let's map AI to normal enterprise app areas.

---

## Slide 4 - Where AI fits in an enterprise app

### Main message

Targeted AI beats generic AI.

### Table

| Enterprise area | Smarter app opportunity |
|---|---|
| Observability | Explain logs, traces, failures, and root cause candidates |
| User experience | Understand user intent and improve product discovery |
| Business operations | Summarize trends, failed searches, and product gaps |
| Integrations | Expose app capabilities as safe tools |
| Workflows | Coordinate specialized agents |
| Deployment | Move the same app model forward with Aspire options |

### Speaker notes

The goal is not one giant chatbot sitting next to the app.

The goal is to add intelligence where the app already has useful information or useful actions.

### Transition

Here is the demo app we will use.

---

## Slide 5 - Demo app: eShopLite

### Main message

eShopLite is the playground for incremental app intelligence.

### Bullets

- Modern .NET eCommerce sample.
- Aspire-based local development.
- Product catalog and search.
- Existing AI scenarios.
- Good base for app modernization demos.
- Good base for extending the same app with smarter features.

### Speaker notes

We are going to keep coming back to the same app.

That is important. This is not a collection of disconnected demos. It is one modernized application that gets smarter step by step.

### Transition

First scenario: the app explains itself.

---

## Slide 6 - Demo 1: The app explains itself

### Title

**Smarter operations: AI over logs and traces**

### Main message

Observability gives us data. AI helps us understand it.

### Bullets

- Read recent application activity.
- Group related failures.
- Summarize root cause candidates.
- Include relevant services and trace IDs.
- Suggest next developer actions.

### Demo scenario

`13-ObservabilityAssistantFoundryLocal`

### Demo prompt

```text
Summarize the last 10 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

### Suggested visual

```text
Logs + traces + metrics
        ↓
Observability Assistant
        ↓
Summary + root cause hints + next actions
```

### Speaker notes

This is where modernization pays off immediately.

Before modernization, the app may not have enough telemetry to make this useful. After modernization, we have logs, traces, metrics, service names, resource names, and app context.

That means we can ask the app to explain what happened.

### Transition

Now let's move from helping developers to helping users.

---

## Slide 7 - Demo 2: Search becomes product discovery

### Title

**Smarter UX: AI over product search**

### Main message

Users search with intent, not database keywords.

### Bullets

- Move beyond exact keyword search.
- Understand natural-language intent.
- Ground answers in product catalog data.
- Explain why each product matches.
- Keep the experience inside the app.

### Demo scenario

`14-ProductDiscoveryCopilot`

### Demo prompts

```text
Find products that are good for walking all day and explain why each result matches.
```

```text
Show me products under $100 that would be useful for a summer trip.
```

### Suggested visual

```text
Keyword search
    ↓
Semantic / intent-based discovery
    ↓
Grounded product explanation
```

### Speaker notes

This is not "we added a chatbot because AI."

This is improving a feature users already understand: search.

The app can now answer the question behind the search.

### Transition

The app is now smarter for developers and users. What about business users?

---

## Slide 8 - Demo 3: Store Intelligence Report

### Title

**Smarter business operations: AI over app signals**

### Main message

Apps collect signals, but they rarely explain them.

### Bullets

- Top searches.
- Failed searches.
- Product opportunities.
- Catalog gaps.
- Operational issues that may affect customers.
- Recommended next actions.

### Demo scenario

`15-StoreIntelligenceReport`

### Demo prompt

```text
Create today's store intelligence report. Include top searches, failed searches, product opportunities, operational issues that may affect customers, and recommended next actions.
```

### Suggested visual

```text
Search events + product data + orders + telemetry
        ↓
Store Intelligence Report
        ↓
Business summary + recommendations
```

### Speaker notes

This is where AI becomes useful for people who do not want to query logs, databases, or dashboards.

The app already generates the signals. The smarter app can summarize what matters.

### Transition

So far, the app is using AI internally. Next, we expose the app capabilities to agents.

---

## Slide 9 - Demo 4: MCP Store Tools

### Title

**Smarter integrations: app capabilities as tools**

### Main message

A modernized app has capabilities that agents can safely use.

### Bullets

Expose app capabilities such as:

- `search_catalog`
- `get_product_details`
- `get_failed_searches`
- `get_store_summary`
- `get_recent_operational_summary`
- `generate_store_intelligence_report`

### Demo scenario

`16-MCPStoreOperationsTools`

### Demo prompt

```text
Customers are searching for travel products. Use the store tools to find matching products, identify failed searches, and summarize product gaps.
```

### Suggested visual

```text
eShopLite capabilities
        ↓
MCP tools
        ↓
Agent uses app safely
```

### Speaker notes

MCP matters here because the app has useful capabilities.

This is the modernization story: once your app has clean services and APIs, you can expose selected capabilities as tools instead of giving an agent uncontrolled access to everything.

### Transition

MCP lets an agent use the app. But enterprise workflows often need multiple agents.

---

## Slide 10 - Demo 5: A2A Store Operations Network

### Title

**Smarter workflows: specialized agents collaborate**

### Main message

Real enterprise workflows need specialized agents, not one giant chatbot.

### Visual

```text
User question
    ↓
Orchestrator Agent
    ├── Catalog Agent
    ├── Observability Agent
    └── Business Insights Agent
    ↓
Recommended action plan
```

### Demo scenario

`17-A2AStoreOperationsNetwork`

### Demo prompt

```text
Customers are searching for travel products but not converting. Investigate the likely reasons and suggest actions.
```

### Bullets

- Catalog Agent checks product coverage.
- Observability Agent checks errors, latency, and traces.
- Business Insights Agent checks searches and conversion signals.
- Orchestrator Agent combines results into an action plan.

### Speaker notes

MCP lets agents use tools.

A2A lets agents collaborate.

This is closer to real enterprise workflows where different systems, teams, and responsibilities need to coordinate.

### Transition

Once we have agents, the next question is where those agents run.

---

## Slide 11 - Hosted Agents evaluation

### Title

**From local agent demo to cloud-ready agent model**

### Main message

Some agents can stay local. Some may become Hosted Agents.

### Bullets

Evaluate:

- Which agents stay local for the session?
- Which agents could become Microsoft Foundry Hosted Agents?
- Which agents need app data?
- Which agents need operational data?
- Which agents need to be deployed independently?
- Which agents need identity, configuration, and cloud resources?

### Speaker notes

For the live demo, we can keep the A2A workflow local.

But architecturally, this is where we evaluate whether some agents should become Microsoft Foundry Hosted Agents and be modeled through Aspire.

The point is not the hosting mechanism. The point is that the modernized app now has agent-ready capabilities.

### Transition

Now let's talk about deployment, but only at the level needed for this session.

---

## Slide 12 - Deploy with Aspire

### Title

**Deployment is not the demo, but it matters**

### Main message

The same app model has deployment options.

### Bullets

- Same AppHost model.
- Local development.
- Publish/deploy options.
- Azure Container Apps.
- Azure App Service.
- Kubernetes / AKS.
- Docker Compose.
- Custom pipelines.

### Speaker notes

The previous session covers Aspire in more depth.

Here, the point is simple: the app model we used locally has a path forward. We are not building a one-off local demo that cannot move anywhere.

### Transition

Let's close with the enterprise pain points this approach addresses.

---

## Slide 13 - Top 5 pain points

### Title

**Why this matters**

### Main message

These are not AI science projects. These are normal enterprise pain points.

### Bullets

1. We have logs, but nobody has time to read them.
2. Users search with intent, not database keywords.
3. Apps collect signals but do not explain them.
4. Integrations still require custom glue.
5. Real workflows need multiple specialized agents.

### Speaker notes

This is the real reason to add AI after modernization.

Not because AI is trendy, but because modernized apps expose the exact signals and capabilities that make AI useful.

### Transition

Final thought.

---

## Slide 14 - Final message

### Main text

```text
Modernization is not the finish line.

It is the foundation that lets us make apps smarter.
```

### Optional second line

```text
Upgrade the app. Then teach it to explain, understand, summarize, expose, and collaborate.
```

### Speaker notes

Once the app is modernized, we can add AI exactly where it helps:

- operations,
- UX,
- business insights,
- app capabilities,
- workflows.

That is the next step after modernization.

## Appendix slides

### Appendix A - Scenario folders

```text
/scenarios/13-ObservabilityAssistantFoundryLocal
/scenarios/14-ProductDiscoveryCopilot
/scenarios/15-StoreIntelligenceReport
/scenarios/16-MCPStoreOperationsTools
/scenarios/17-A2AStoreOperationsNetwork
```

### Appendix B - Out of scope

```text
- Aspire MCP server / AI coding-agent pitch
- Smart Components
- eShopSupport
- eShopOnWeb
- Live deployment demo
- DevUI as production feature
```

### Appendix C - References

- eShopLite: https://github.com/Azure-Samples/eShopLite
- Aspire 13.4 Hosted Agents: https://aspire.dev/whats-new/aspire-13-4/#foundry-hosted-agent-commands-and-fixes
- Aspire deployment: https://aspire.dev/deployment/
- Microsoft Agent Framework: https://learn.microsoft.com/en-us/agent-framework/overview/
- A2A integration: https://learn.microsoft.com/en-us/agent-framework/integrations/a2a
- DevUI: https://learn.microsoft.com/en-us/agent-framework/devui/
