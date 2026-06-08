# Slide Content - App Modernization Done. Now Let's Make It Smarter

## Deck guidance

This file contains slide-ready content: slide titles, bullets, suggested visuals, speaker notes, demo prompts, transitions, and fallback notes.

The tone should be practical, enterprise-friendly, and developer-first.

The deck should not feel like an AI hype talk. It should feel like:

> We upgraded the app. Here are practical ways to make it smarter.

## Design and readability constraints for v02

- Keep the opening structure locked:
  - Slide 1 = template slide 1 (unchanged).
  - Slide 2 = speaker intro (Bruno headshot + session title).
  - Slide 3 = template slide 3 style + modernization-to-smarter bridge pitch.
- Prefer visual storytelling layouts over two equal text boxes.
- Avoid dense paragraph blocks on slides; move detail to speaker notes.
- Target larger room-friendly typography:
  - Title: 40-48 pt
  - Section headings: 24-30 pt
  - Body: 20-24 pt minimum where possible
- Every demo slide should show either a process flow, evidence-to-outcome sequence, or one strong visual anchor.

## Slide 1 - Template opener (locked)

### Instruction

Use slide 1 from:

`slides/NET Day June 16.pptx`

Do not redesign this slide. Keep branding and layout from template.

### Speaker notes

Most modernization talks focus on getting the app onto a newer stack. That is important, but today I want to start right after that moment.

The app is already modernized.

Now what can we do to make it smarter?

### Transition

Let's start with what the upgraded app gives us.

---

## Slide 2 - Speaker intro (Bruno + session)

### Layout

- Bruno headshot (from `slides/Bruno Capuano - Headshopt.jpg`)
- Session title:
  - **App Modernization Done. Now Let's Make It Smarter**
- Short subline:
  - “From modernization foundations to practical app intelligence with eShopLite”

### Main message

Set context quickly: this session starts where modernization talks usually end.

### Speaker notes

We already saw modernization patterns, Aspire usage, and platform setup.
Now we are going to use that foundation to make the app smarter in concrete, scoped ways.

### Transition

So the question is not "how do we add a chatbot?" The question is "where can AI improve the app?"

---

## Slide 3 - Modernization bridge statement (template style)

### Instruction

Use slide 3 style from:

`slides/NET Day June 16.pptx`

### Main message

We already saw how to modernize an app and use Aspire.  
Now that foundation is done, let's make our app smarter.

### Supporting line

Modernization gave us structure, telemetry, data, and boundaries.  
Now we use those assets for targeted intelligence.

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

### Visual-first content

- Use this GIF as the hero visual:
  - `images/06eShopLite-SearchSemantic.gif`
- Keep on-slide text minimal:
  - “One app, one flow, progressively smarter outcomes.”
  - “We’ll use this exact experience throughout the demos.”

### Speaker notes

I will focus on this semantic search experience only, so the audience can follow one continuous story.

This is not a set of disconnected mini-demos. It is one modernized app becoming smarter step by step.

### Transition

First scenario: the app understands what users actually mean.

---

## Slide 6 - Demo 1: Search becomes product discovery (code walkthrough)

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

`14-ProductDiscoveryCopilot` — **code walkthrough, no app run.**

### Code to walk

```text
src/Products/Memory/MemoryContext.cs
```

### Walkthrough talking points

- **Startup grounding:** every catalog product is embedded into an in-memory vector store
  (`InitMemoryContextAsync`).
- **Query grounding:** the shopper's question is embedded and matched against the catalog
  (top 3).
- **Honesty gate:** `Score > 0.3` keeps only real, similar products — no match means no
  invented products.
- **Grounded prompt:** only the matched products + the question are sent to the chat model,
  alongside an on-catalog system prompt.

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

This is improving a feature users already understand: search — and we show *how* it stays
honest, in code. The model only ever sees the products the vector search returned.

### Transition

That used a cloud model. Now let's bring AI local — and point it at logs and traces.

---

## Slide 7 - Demo 2: The app explains itself (local AI)

### Title

**Smarter operations: local AI over logs and traces**

### Main message

Observability gives us data. AI helps us understand it — running locally with Foundry Local.

Demo 2 uses three Aspire services: `products`, `store`, and `observabilityassistant`.

### Storytelling structure (replace two-text-box layout)

Use a 4-step flow with icons/arrows:

1. **Trigger** - “Checkout errors increased after deployment”
2. **Evidence** - Logs + traces + failing service calls across products/store/observabilityassistant
3. **AI explanation** - Grouped incidents + likely root cause + trace IDs
4. **Action** - Top 3 checks for the developer

### Demo scenario

`13-ObservabilityAssistantFoundryLocal` — runs the model **locally** with Foundry Local.

### Demo prompt

```text
Summarize the last 10 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

### Window flow callout

- Run the same analysis at **5 / 10 / 15 / 30 minutes**.
- Trigger from Store UI.
- Clarify that Store displays findings generated by `observabilityassistant` backend.

### Local AI callout

- The model runs on the developer's machine via **Foundry Local** — no telemetry leaves the box.
- A natural fit for logs and traces, which are sensitive and high-volume.

### Suggested visual

```text
Incident signal
    ↓
Observability Assistant (Foundry Local)
    ↓
Root cause narrative
    ↓
Next actions
```

### Speaker notes

Tell this like an incident story, not a feature checklist.

We just made the user experience smarter with a cloud model. Now we keep AI on the box: a
local model reads telemetry and explains what happened, with nothing leaving the machine.

### Transition

Now the app is smarter for users and for operations. What about business users?

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

**Hosted Agents decision point: what stays local, what moves to cloud**

### Main message

Use this slide as a decision framework, not a binary split diagram.

### Content style

Use a decision matrix (criteria × candidate agents), not two plain text boxes.

Decision criteria:
- Needs independent scaling
- Needs managed identity/security boundary
- Needs persistent cloud context/data
- Needs independent lifecycle from app deploy

### Speaker notes

For the live demo, we can keep the A2A workflow local.

But architecturally, this is where we evaluate whether some agents should become Microsoft Foundry Hosted Agents and be modeled through Aspire.

The point is not the hosting mechanism. The point is that the modernized app now has agent-ready capabilities.

### Transition

Now let's talk about deployment, but only at the level needed for this session.

---

## Slide 12 - Deploy with Aspire

### Title

**Path to production**

### Main message

The local app model we demoed has a clear path to production deployment options.

### Bullets

- Same AppHost model from local to production
- `aspire publish` / `aspire deploy` as transition points
- Azure Container Apps / App Service / AKS / Docker Compose
- Keep this as architecture direction (no live deployment demo)

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
