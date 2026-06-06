# PRD: eShopLite Agentic Modernization Scenarios

**Session:** App Modernization Done. Now Let's Make It Smarter.  
**Event:** .NET Day on Agentic Modernization  
**Event URL:** https://developer.microsoft.com/en-us/reactor/events/27243/  
**Repository:** https://github.com/Azure-Samples/eShopLite  
**Required working branch:** `bruno-NETAgenticModernizationSession`  
**PRD version:** v3 - Agentic modernization focus, Hosted Agents evaluation, no Aspire MCP/coding-agent pitch  
**Target session docs folder:** `/docs/26 06 16 NET Agentic Modernization/`  
**Owner / speaker:** Bruno Capuano  
**Status:** Draft PRD  
**Target platform:** .NET 10, latest NuGet packages, latest Aspire packages and flows  

---

## 1. Executive summary

The goal of this work is to add a new set of **agentic modernization demo scenarios** to the eShopLite repository. The demos support a 30-minute session titled **"App Modernization Done. Now Let's Make It Smarter."**

The core message is:

> Modernization is not the finish line. It is the foundation that makes AI useful.

The session assumes the app is already modernized: it runs on modern .NET, uses Aspire for orchestration, has service boundaries, emits logs/traces/metrics, and can be deployed using the current Aspire deployment model. The next step is to add AI where the modernized app already has valuable signals: logs, traces, product data, search behavior, app APIs, and operational workflows.

This PRD defines:

1. The new demo scenarios to add under `/scenarios`.
2. The implementation instructions for each scenario.
3. The required working branch: `bruno-NETAgenticModernizationSession`.
4. The scenario derivation rule: new scenarios should start from `01 - Semantic Search` unless the scenario requires a more specific existing baseline such as MCP, A2A, or deployment.
5. The documentation folder to add under `/docs/26 06 16 NET Agentic Modernization/`.
6. The session narrative, speaker script, demo script, slide content, and validation checklist.
7. The acceptance criteria to keep the demos repeatable, short, and safe for a live session.

---

## 2. Context and research anchors

### 2.1 Event context

The Microsoft Reactor event page describes **.NET Day on Agentic Modernization** as a livestream focused on helping .NET developers modernize existing apps without a rewrite, then build AI-powered experiences on Azure. Bruno's session is listed as **"App Modernization Done. Now Let's Make It Smarter."** and is positioned as a practical 30-minute session about adding AI to an existing Azure-hosted application using Microsoft Foundry, including log analysis, insights, and smarter reports.

Source: https://developer.microsoft.com/en-us/reactor/events/27243/

### 2.2 eShopLite baseline

The eShopLite repository is a reference .NET eCommerce application with scenarios for semantic search, MCP, reasoning models, vector databases, realtime audio, A2A, GitHub Models, Azure Functions, and more. The repository currently lists scenarios 01–12.

Source: https://github.com/Azure-Samples/eShopLite

Current public scenario baseline:

| Existing scenario | Area |
|---|---|
| 01 - Semantic Search | Keyword + semantic search |
| 02 - Azure AI Search | Azure AI Search + vector search |
| 03 - Realtime Audio | Realtime audio in the eCommerce app |
| 04 - Chroma DB | Chroma vector database |
| 05 - DeepSeek-R1 | Reasoning model integration |
| 06 - Model Context Protocol (MCP) | MCP server and client |
| 07 - Agents Concurrent | Concurrent agent orchestration |
| 08 - SQL Server 2025 | SQL vector search and indexes |
| 09 - Azure App Service | Azure App Service deployment |
| 10 - A2A Network | Agent-to-agent communication |
| 11 - GitHub Models | Local-first dev with GitHub Models and Azure OpenAI when deployed |
| 12 - Azure Functions | Functions façade and alternate deployment boundary |

The new scenarios in this PRD should start at **13** to avoid disrupting existing scenario numbering.

### 2.3 Aspire anchors

The session should align with the latest Aspire direction, but the pitch must stay focused on **making upgraded .NET applications smarter**.

- Aspire 13.4 includes expanded app-model capabilities, server-side log and telemetry search, deployment improvements, Foundry hosted agent updates, Kubernetes/AKS deployment APIs, and dashboard/editor improvements.  
  Source: https://aspire.dev/whats-new/aspire-13-4/

- Aspire 13.4 introduces a relevant Hosted Agents angle: project or app resources that implement the Microsoft Foundry Hosted Agents protocols can be modeled as hosted agents, interacted with through the Aspire dashboard, and deployed with Aspire. This should be evaluated for agent-based scenarios such as the A2A Store Operations Network.  
  Source: https://aspire.dev/whats-new/aspire-13-4/#foundry-hosted-agent-commands-and-fixes

- Aspire deployment remains slide-only in this session. Use the deployment story to show that the same modernized app model can move to cloud targets using Aspire publish/deploy flows and current deployment capabilities.  
  Source: https://aspire.dev/deployment/

Important scope note:

- Do **not** pitch "Aspire + AI coding agents" or the Aspire MCP server in this session. Those topics are better aligned with the dedicated Aspire session before this one.
- This session should use Aspire as the modernization foundation, then focus on how AI makes the application itself smarter for developers, users, operators, and business stakeholders.

---

## 3. Product vision

### 3.1 Session focus guardrails

The core message for the session is:

> The app has been modernized. Now we can make it smarter.

The session should not become an Aspire tooling session. Aspire is the foundation that gives us orchestration, telemetry, deployment shape, and a clear app model. The session value is showing what AI can do **after** the app has been upgraded.

In scope for the pitch:

- AI over application logs, traces, and operational signals.
- AI over search, user experience, and product discovery.
- AI over business activity and app-generated data.
- MCP as a way for the app to expose business/application capabilities as tools.
- A2A as a way for app-specific agents to collaborate across boundaries.
- Microsoft Agent Framework as the app-code layer for agents and workflows.
- DevUI as an optional development/testing aid for agents, not a production feature and not a main pitch section.
- Foundry Hosted Agents as an evaluation path for agent scenarios.

Out of scope for the pitch:

- Aspire MCP server for AI coding agents.
- "Aspire + AI coding agents" as a session topic.
- A deep dive into Aspire dashboard AI Agents entry points.
- A live deployment demo.
- Smart Components or older/outdated sample apps.

After modernization, enterprise apps usually have better structure, better deployment, better telemetry, and better developer experience. But many of the actual user and operational pain points remain:

- Developers still read logs manually.
- Product search still behaves like a database query.
- Business users still ask for manual reports.
- APIs are still consumed through custom glue.
- Production workflows still cross multiple systems and owners.

The new eShopLite scenarios should show how AI can be added **incrementally** to specific parts of a modernized app without rewriting the whole system.

The desired audience reaction:

> “I do not need to rebuild my app to make it smarter. If my app is already observable, deployable, and well-structured, I can add AI exactly where it helps.”

---

## 4. Goals

### 4.1 Session goals

1. Show a practical post-modernization story for .NET apps.
2. Use eShopLite as the common demo playground.
3. Demonstrate concrete AI features added to an already modernized app.
4. Connect app modernization, Aspire, Microsoft Foundry, Foundry Local, MCP, and A2A into one coherent story.
5. Keep deployment as a slide-based closing section focused on what Aspire provides.

### 4.2 Repository goals

1. Add new scenario folders under `/scenarios`.
2. Add session documentation under `/docs/26 06 16 NET Agentic Modernization/`.
3. Keep every scenario runnable independently.
4. Avoid breaking existing scenarios 01–12.
5. Use .NET 10 and current package versions already applied in the repository.
6. Prefer shared abstractions and reusable services where possible, but avoid over-engineering the sample.

### 4.2.1 Branching and source-scenario rules

All implementation work for this PRD must be done in a new branch named:

```bash
git checkout -b bruno-NETAgenticModernizationSession
```

The branch must contain both the new scenario folders and the new session documentation folder.

Default source scenario rule:

- New scenarios must start from **`01 - Semantic Search`** as the default baseline.
- This keeps the session coherent because the audience sees the same base eShopLite app evolve through each demo.
- A scenario may start from a different existing scenario only when it requires scenario-specific infrastructure that is already present elsewhere.
- When a scenario does not start from `01 - Semantic Search`, document the reason in that scenario's `README.md`.

Recommended source mapping:

| New scenario | Default/source baseline | Reason |
|---|---|---|
| `13-ObservabilityAssistantFoundryLocal` | Start from `01 - Semantic Search` | Uses the modern app baseline, then adds telemetry summarization. |
| `14-ProductDiscoveryCopilot` | Start from `01 - Semantic Search` | Direct continuation of the search experience. |
| `15-StoreIntelligenceReport` | Start from `01 - Semantic Search` | Uses the same catalog/search baseline plus report generation. |
| `16-MCPStoreOperationsTools` | Prefer `01 - Semantic Search`; reuse patterns from `06 - Model Context Protocol (MCP)` as needed | MCP-specific server/client plumbing may come from scenario 06. |
| `17-A2AStoreOperationsNetwork` | Prefer `01 - Semantic Search`; reuse patterns from `10 - A2A Network` and/or https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents as needed | A2A-specific orchestration may require the existing A2A baseline. |
| Deploy with Aspire docs | Docs only; reference `09 - Azure App Service` and current Aspire deployment docs | Slide-based only; no new live deployment scenario required. |


### 4.3 Demo goals

Each demo should be explainable in under 5 minutes and runnable locally from the scenario folder.

The live demo path should be:

1. Start with eShopLite running in Aspire.
2. Show the modern baseline: app, services, dashboard, logs, traces.
3. Add AI to observability.
4. Add AI to product discovery.
5. Add AI to business operations.
6. Expose app capabilities through MCP.
7. Show A2A as the next step after MCP.
8. Close with deployment options from Aspire.

---

## 5. Non-goals

- Do not include the Aspire MCP server or Aspire AI coding-agent workflow as part of the session pitch. This belongs to the dedicated Aspire session.
- Do not position DevUI as a production feature or a main audience takeaway; use it only as an optional development/testing aid for agent scenarios.
- Do not turn the deploy section into a live demo. Deployment remains slide-only and focused on Aspire deployment options.
1. Do not use Smart Components.
2. Do not use eShopSupport.
3. Do not use eShopOnWeb.
4. Do not rebuild eShopLite architecture from scratch.
5. Do not create a giant generic chatbot as the main demo.
6. Do not require production cloud deployment for every demo.
7. Do not make the deployment section a live demo; keep it slide-based.
8. Do not introduce unsupported or stale packages.
9. Do not add real production secrets to the repo.
10. Do not require paid model calls for every scenario if a local path is possible.

---

## 6. Proposed new scenario folders

Add the following folders under `/scenarios`.

Important implementation rule: create these folders from `01 - Semantic Search` by default. Use another existing scenario only when the new scenario specifically needs that baseline, such as MCP, A2A, or deployment-specific infrastructure.

```text
/scenarios/13-ObservabilityAssistantFoundryLocal
/scenarios/14-ProductDiscoveryCopilot
/scenarios/15-StoreIntelligenceReport
/scenarios/16-MCPStoreOperationsTools
/scenarios/17-A2AStoreOperationsNetwork
```

Add a shared docs folder:

```text
/docs/26 06 16 NET Agentic Modernization/
```

---

## 7. Scenario overview

| Scenario | Name | Main idea | Demo type | Key technologies |
|---|---|---|---|---|
| 13 | Observability Assistant with Foundry Local | Summarize logs, traces, and incidents locally | Live demo | Aspire, OpenTelemetry, Foundry Local, Microsoft.Extensions.AI |
| 14 | Product Discovery Copilot | Turn search into natural-language product discovery | Live demo | Semantic search, Azure AI Search or local vector search, Microsoft.Extensions.AI |
| 15 | Store Intelligence Report | Summarize business and operational activity | Live demo | App data, telemetry, AI summarization, reports |
| 16 | MCP Store Operations Tools | Expose eShopLite capabilities as MCP tools | Live demo | MCP, Aspire, app APIs, tool calling |
| 17 | A2A Store Operations Network | Show specialized agents collaborating across boundaries | Architecture demo / optional live demo | A2A, Microsoft Agent Framework, Foundry hosted agents or local NeMo integration |
| Docs only | Deploy with Aspire | Explain deployment options and deployment story | Slide only | `aspire publish`, `aspire deploy`, Azure, App Service, ACA, Kubernetes, Docker Compose |

---

# 8. Scenario 13: Observability Assistant with Foundry Local

## 8.1 Story

The app is modernized. It emits logs, traces, and metrics. Aspire makes local orchestration and dashboard inspection easy. But developers still need to read logs manually.

This scenario adds an AI-powered local observability assistant that summarizes application telemetry and explains likely causes of failures.

Core line:

> Modernization gave us telemetry. AI turns telemetry into understanding.

## 8.2 User stories

As a developer, I want to ask what happened in the app during the last few minutes, so I do not manually inspect every log line.

As a developer, I want errors grouped by service and likely cause, so I can quickly decide where to investigate.

As a demo attendee, I want to see that AI can help with a real modernization pain point without changing the entire app.

## 8.3 Functional requirements

Create scenario folder:

```text
/scenarios/13-ObservabilityAssistantFoundryLocal
```

Recommended project structure:

```text
/scenarios/13-ObservabilityAssistantFoundryLocal/
  README.md
  src/
    eShopLite.AppHost/
    eShopLite.WebApp/
    eShopLite.ApiService/
    eShopLite.ObservabilityAssistant/
  docs/
    demo-script.md
    architecture.md
    troubleshooting.md
```

The scenario must include:

1. An Aspire AppHost that orchestrates the eShopLite services and the new observability assistant service.
2. A new service named `eShopLite.ObservabilityAssistant`.
3. An endpoint to summarize logs and incidents.
4. A local model path using Foundry Local.
5. A fallback or abstraction that allows the model provider to be replaced with Azure OpenAI or GitHub Models when needed.
6. A simple UI action in the web app, or a minimal diagnostics page, to call the assistant.
7. Sample generated incidents for deterministic demos.

## 8.4 Suggested endpoints

```http
GET /observability/summary?windowMinutes=10
GET /observability/errors?windowMinutes=10
GET /observability/trace/{traceId}/explain
POST /observability/demo/fail-search
POST /observability/demo/fail-checkout
```

## 8.5 Suggested request/response contract

```csharp
public sealed record ObservabilitySummaryRequest(
    int WindowMinutes,
    string? ResourceName,
    string? Severity,
    bool IncludeTraceHints);

public sealed record ObservabilitySummaryResponse(
    DateTimeOffset GeneratedAt,
    int WindowMinutes,
    IReadOnlyList<ServiceIssueGroup> Groups,
    string ExecutiveSummary,
    IReadOnlyList<string> RecommendedActions,
    IReadOnlyList<string> TraceIds);

public sealed record ServiceIssueGroup(
    string ServiceName,
    string Severity,
    int Count,
    string Summary,
    string ProbableCause,
    IReadOnlyList<string> ExampleMessages);
```

## 8.6 Data sources

Preferred local demo input should be deterministic and easy to control:

1. Structured logs emitted by the scenario services.
2. Optional exported logs from the Aspire dashboard or OpenTelemetry pipeline.
3. A scenario-local log buffer service for deterministic demo runs.
4. Optional trace metadata when available.

For live reliability, the scenario should not depend on scraping the Aspire dashboard UI. Use app-side telemetry capture or exported telemetry where possible.

## 8.7 Prompt shape

Use a constrained prompt with a strict output contract.

```text
You are an observability assistant for a .NET eCommerce application.
Summarize the telemetry data provided below.
Group issues by service.
Identify the most likely cause.
Return only JSON using the provided schema.
Do not invent services, trace IDs, or errors that are not in the input.
```

## 8.8 Demo flow

1. Start the scenario with Aspire.
2. Open the eShopLite web app.
3. Run a normal product search.
4. Trigger a demo failure using `/observability/demo/fail-search`.
5. Open the Aspire dashboard and show logs/traces.
6. Open the Observability Assistant page.
7. Click **Summarize last 10 minutes**.
8. Show grouped issues, likely cause, trace IDs, and recommended next actions.
9. Explain that this is local-first with Foundry Local.

## 8.9 Acceptance criteria

1. The scenario runs locally from its own folder.
2. The AppHost starts all resources successfully.
3. The assistant can summarize deterministic sample failures.
4. The demo does not require a cloud deployment.
5. The README explains how to configure Foundry Local.
6. The assistant does not expose secrets in prompts or output.
7. The output includes trace IDs only if present in input data.
8. The demo can be completed in under 5 minutes.

---

# 9. Scenario 14: Product Discovery Copilot

## 9.1 Story

The app already has search. Modernization makes the app easier to evolve. Now AI can improve a feature users already understand: product search.

Core line:

> Do not add a chatbot because “AI”. Improve a feature users already use every day.

## 9.2 User stories

As a shopper, I want to search using natural language, so I can describe intent instead of exact keywords.

As a shopper, I want product results explained, so I understand why they match my request.

As a developer, I want to reuse existing semantic search/vector search capabilities, so the AI feature builds on the modernized app instead of bypassing it.

## 9.3 Functional requirements

Create scenario folder:

```text
/scenarios/14-ProductDiscoveryCopilot
```

Recommended project structure:

```text
/scenarios/14-ProductDiscoveryCopilot/
  README.md
  src/
    eShopLite.AppHost/
    eShopLite.WebApp/
    eShopLite.ApiService/
    eShopLite.ProductDiscovery/
  docs/
    demo-script.md
    prompts.md
    evaluation.md
```

The scenario must include:

1. Natural-language search input.
2. Product matching using existing semantic search or vector search.
3. AI-generated explanation for why each product matches.
4. Grounding: the model must only use product data returned by the search layer.
5. Clear UI labeling that answers are generated from the catalog.
6. Sample prompts for repeatable demo runs.

## 9.4 Example prompts

```text
Find something comfortable for walking all day.
```

```text
Show me products under $100 that are useful for travel.
```

```text
I need a gift for someone who likes hiking and coffee.
```

```text
Compare these products and explain which one is best for a weekend trip.
```

## 9.5 Suggested contracts

```csharp
public sealed record ProductDiscoveryRequest(
    string UserQuery,
    decimal? MaxPrice,
    int Top,
    bool IncludeExplanation);

public sealed record ProductDiscoveryResponse(
    string UserQuery,
    IReadOnlyList<ProductDiscoveryResult> Results,
    string? Summary,
    IReadOnlyList<string> FollowUpQuestions);

public sealed record ProductDiscoveryResult(
    string ProductId,
    string Name,
    decimal Price,
    string MatchReason,
    double? Score);
```

## 9.6 Demo flow

1. Show keyword search with a simple query.
2. Show a natural-language query.
3. Show product results with match explanations.
4. Show that the AI answer cites product fields from the catalog.
5. Mention that this can use local vector search, Azure AI Search, SQL Server 2025 vector search, or other eShopLite search scenarios depending on the chosen backend.

## 9.7 Acceptance criteria

1. The scenario reuses existing catalog/search logic where possible.
2. The AI explanation is grounded in returned product data.
3. The UI makes it clear which content is AI-generated.
4. The scenario supports at least three deterministic demo prompts.
5. The demo can be completed in under 4 minutes.

---

# 10. Scenario 15: Store Intelligence Report

## 10.1 Story

A modernized app produces more signals: searches, failed searches, product views, checkout errors, orders, and telemetry. But those signals are often buried in logs, dashboards, and databases.

This scenario adds a daily AI-generated store intelligence report.

Core line:

> Modern apps generate signals. AI converts signals into decisions.

## 10.2 User stories

As a store manager, I want a daily summary of what happened in the store, so I can act without manually reading dashboards.

As a product owner, I want to know which searches failed, so I can improve the catalog.

As an engineer, I want business activity and operational issues summarized together, so I can connect technical failures with user impact.

## 10.3 Functional requirements

Create scenario folder:

```text
/scenarios/15-StoreIntelligenceReport
```

Recommended project structure:

```text
/scenarios/15-StoreIntelligenceReport/
  README.md
  src/
    eShopLite.AppHost/
    eShopLite.WebApp/
    eShopLite.ApiService/
    eShopLite.StoreIntelligence/
  docs/
    demo-script.md
    report-schema.md
    sample-output.md
```

The scenario must include:

1. A report endpoint or page.
2. A deterministic sample data generator.
3. A summary of catalog activity.
4. A summary of search activity.
5. A summary of failed searches or low-confidence searches.
6. A summary of operational issues that may affect business outcomes.
7. Recommended actions.
8. A structured report schema.

## 10.4 Suggested endpoint

```http
GET /store-intelligence/today
GET /store-intelligence/report?date=2026-06-16
POST /store-intelligence/demo/generate-activity
```

## 10.5 Suggested report schema

```csharp
public sealed record StoreIntelligenceReport(
    DateOnly ReportDate,
    string ExecutiveSummary,
    IReadOnlyList<string> TopSearches,
    IReadOnlyList<string> FailedSearches,
    IReadOnlyList<ProductSignal> ProductSignals,
    IReadOnlyList<OperationalSignal> OperationalSignals,
    IReadOnlyList<string> RecommendedActions);

public sealed record ProductSignal(
    string ProductId,
    string ProductName,
    string SignalType,
    string Summary);

public sealed record OperationalSignal(
    string ServiceName,
    string IssueType,
    string BusinessImpact,
    string RecommendedAction);
```

## 10.6 Example generated report

```text
Today customers searched frequently for “travel shoes”, “light backpack”, and “summer jacket”.
Three high-volume searches returned weak results: “waterproof laptop bag”, “airport hoodie”, and “kids hiking bottle”.
Search latency increased during the same period because the semantic search API returned repeated timeout warnings.
Recommended actions:
1. Review catalog gaps for waterproof travel products.
2. Check semantic search service latency.
3. Add product tags for travel-related items.
```

## 10.7 Demo flow

1. Generate demo activity.
2. Show raw activity/search/error data.
3. Open the Store Intelligence Report page.
4. Generate the summary.
5. Show recommended product and engineering actions.
6. Explain the difference between dashboards and summaries.

## 10.8 Acceptance criteria

1. The scenario works with deterministic sample data.
2. The report includes business and operational signals.
3. The model output is structured and parseable.
4. The report does not invent products or services.
5. The demo can be completed in under 4 minutes.

---

# 11. Scenario 16: MCP Store Operations Tools

## 11.1 Story

After modernization, the app has cleaner services and capabilities. MCP lets agents discover and use those capabilities safely.

Core line:

> MCP is where the modernized app becomes useful to agents.

## 11.2 User stories

As an AI assistant, I need safe tools to query catalog, search, and operational data.

As a developer, I want to expose selected capabilities without giving an agent unrestricted access to the app.

As a demo attendee, I want to see how existing app APIs can become agent-ready tools.

## 11.3 Functional requirements

Create scenario folder:

```text
/scenarios/16-MCPStoreOperationsTools
```

Recommended project structure:

```text
/scenarios/16-MCPStoreOperationsTools/
  README.md
  src/
    eShopLite.AppHost/
    eShopLite.WebApp/
    eShopLite.ApiService/
    eShopLite.McpServer/
    eShopLite.McpClient.Console/
  docs/
    demo-script.md
    tools.md
    security-notes.md
```

The scenario must include:

1. An MCP server exposing eShopLite tools.
2. A simple MCP client or documented usage with a compatible client.
3. Tool descriptions with clear input/output schemas.
4. Read-only tools by default.
5. Optional demo-only command tools with explicit names and guardrails.
6. Documentation explaining how this differs from a generic chatbot.

## 11.4 Required MCP tools

```text
search_catalog
get_product_details
get_failed_searches
get_store_intelligence_report
get_recent_errors
get_current_app_health
```

## 11.5 Optional demo tools

```text
generate_demo_activity
trigger_demo_search_failure
```

These must be clearly marked as demo-only.

## 11.6 Tool contracts

Example `search_catalog` contract:

```json
{
  "name": "search_catalog",
  "description": "Search the eShopLite product catalog using a user query.",
  "input": {
    "query": "string",
    "top": "number"
  },
  "output": {
    "results": [
      {
        "productId": "string",
        "name": "string",
        "price": "number",
        "description": "string"
      }
    ]
  }
}
```

## 11.7 Demo flow

1. Start the scenario with Aspire.
2. Show the web app.
3. Show the MCP server running as a resource.
4. Use the MCP client to call `search_catalog`.
5. Use the MCP client to call `get_store_intelligence_report`.
6. Use the MCP client to call `get_recent_errors`.
7. Explain that the app now exposes safe, discoverable capabilities for agents.

## 11.8 Acceptance criteria

1. MCP tools are read-only unless explicitly demo-only.
2. Tool names are clear and stable.
3. Tool outputs are structured.
4. The scenario README documents how to connect a client.
5. The demo can be completed in under 5 minutes.

---

# 12. Scenario 17: A2A Store Operations Network

## 12.1 Story

MCP lets an agent use app tools. A2A lets agents collaborate across boundaries.

This scenario shows how a modernized app can participate in an agent network where specialized agents work together.

Core line:

> MCP is “agent uses app tools.” A2A is “agents collaborate across boundaries.”

## 12.2 User stories

As a business user, I want a multi-agent workflow to answer a question that spans product data, operational telemetry, and business activity.

As a developer, I want each agent to have a focused responsibility.

As an architect, I want to see how local and remote agents could collaborate around a modern .NET app.

## 12.3 Functional requirements

Create scenario folder:

```text
/scenarios/17-A2AStoreOperationsNetwork
```

Recommended project structure:

```text
/scenarios/17-A2AStoreOperationsNetwork/
  README.md
  src/
    eShopLite.AppHost/
    eShopLite.WebApp/
    eShopLite.ApiService/
    eShopLite.Agents.CatalogAgent/
    eShopLite.Agents.ObservabilityAgent/
    eShopLite.Agents.BusinessInsightsAgent/
    eShopLite.Agents.Orchestrator/
  docs/
    demo-script.md
    agent-cards.md
    sequence-diagram.md
    foundry-hosted-agent-option.md
    nemo-local-agent-option.md
```

The scenario must include:

1. At least three specialized agents.
2. A simple orchestrator that coordinates the agents.
3. Agent descriptions or agent cards.
4. A demo question that requires more than one agent.
5. An architecture option for Foundry hosted agents.
6. An architecture option for local integration with an external agent framework such as NVIDIA NeMo through an A2A pattern.

Reference sample for local NeMo + Microsoft Agent Framework + A2A inspiration:

https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents

## 12.4 Required agents

### Catalog Agent

Responsibilities:

- Search product catalog.
- Identify product gaps.
- Find matching or related products.

### Observability Agent

Responsibilities:

- Summarize recent errors.
- Explain trace/log patterns.
- Identify technical issues affecting user experience.

### Business Insights Agent

Responsibilities:

- Summarize failed searches.
- Explain business impact.
- Recommend catalog or operations actions.

### Orchestrator Agent

Responsibilities:

- Receive user question.
- Route work to specialized agents.
- Merge the final answer.
- Keep response grounded in tool outputs.

## 12.5 Example demo question

```text
Why are customers not finding good travel products today, and what should we do next?
```

Expected workflow:

1. Orchestrator receives the question.
2. Catalog Agent searches for travel-related products.
3. Business Insights Agent reads failed searches and product activity.
4. Observability Agent checks search service health.
5. Orchestrator combines the answer:
   - product gaps,
   - search behavior,
   - operational issues,
   - recommended next steps.

## 12.6 Foundry Hosted Agents evaluation

Add a documentation page and a small technical evaluation explaining whether the agents in this scenario can be exposed and deployed as Microsoft Foundry Hosted Agents through Aspire.

Recommended doc:

```text
/scenarios/17-A2AStoreOperationsNetwork/docs/foundry-hosted-agent-evaluation.md
```

Evaluation goal:

- Decide whether the A2A agents should stay local for the live demo, be deployed as Hosted Agents, or support both paths.
- Keep the live session focused on app intelligence, not deployment mechanics.
- Use the Hosted Agents path as the "cloud-ready extension" of the A2A story.

Content must explain:

1. Which agents could become hosted agents:
   - Catalog Agent
   - Observability Agent
   - Business Insights Agent
   - Orchestrator Agent
   - Any local NeMo-style agent from https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents
2. Which agents should remain local for the conference demo.
3. Which agents would benefit from being deployed as Foundry Hosted Agents.
4. How each candidate agent implements, or would need to implement, the Microsoft Foundry Hosted Agents responses/invocations protocols.
5. How Aspire can model the hosted-agent resources in the AppHost.
6. Which configuration values are required for local development and cloud deployment.
7. Which identities, roles, and environment variables are required.
8. What is demo-ready now versus what remains a conceptual extension.
9. Any API changes required by the current Aspire package version, including verification of current hosted-agent APIs before implementation.

Hosted Agents acceptance criteria:

- The scenario includes an explicit Hosted Agent decision table.
- The documentation uses the current Aspire version installed in the repository as the implementation reference.
- If Aspire 13.4 APIs are used, verify current hosted-agent API names during implementation.
- The PRD notes that Aspire 13.4 documentation describes hosted agents as project/app resources that can be interacted with through the Aspire dashboard and deployed with Aspire.
- The PRD also notes the Aspire 13.4 breaking-change guidance around hosted-agent API names, including `WithComputeEnvironment` and `AddPromptAgent` ordering, so implementers validate the exact API before coding.
- The live demo may remain local even if the documentation explains the Hosted Agent path.

## 12.7 Local NeMo integration option

Add a documentation page explaining how this scenario can integrate with a local NeMo agent using the pattern from:

https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents

Recommended doc:

```text
/scenarios/17-A2AStoreOperationsNetwork/docs/nemo-local-agent-option.md
```

Content should explain:

1. How a local NeMo data analysis agent can participate.
2. How a Microsoft Agent Framework action agent can participate.
3. How A2A communication works at a high level.
4. How Aspire orchestrates the local demo.
5. What should remain conceptual if time is limited.

## 12.7.1 Microsoft Agent Framework positioning

For agent-based scenarios, Microsoft Agent Framework should be positioned as the application-code layer for agents and workflows.

Use this framing:

```text
Microsoft Agent Framework = build agents and workflows in app code.
MCP = expose app capabilities as safe tools.
A2A = expose agents so other agents can discover and collaborate with them.
Aspire = orchestrate, observe, and deploy the app resources.
```

Implementation guidance:

1. Prefer Agent Framework for .NET agent implementations when the scenario needs typed workflows, tool use, state/session handling, middleware, telemetry, or explicit multi-agent orchestration.
2. Keep the live demo simple: one orchestrator agent plus two or three specialized agents is enough.
3. Avoid creating a generic "chatbot" story. Every agent must map to an app modernization value:
   - explain operational signals,
   - improve product discovery,
   - summarize business activity,
   - coordinate app-specific tasks.
4. Any Agent Framework agent that participates in A2A must document:
   - agent name,
   - responsibility,
   - exposed endpoint,
   - agent card metadata,
   - required configuration,
   - demo prompt,
   - expected output.
5. Do not pitch this as a framework comparison. The session story is about smarter modernized apps.

## 12.7.2 DevUI integration guidance

DevUI may be used as an optional local development/testing aid for agents and workflows.

DevUI is not part of the main session pitch and is not a production feature. Add a short documentation page only if it helps developers validate the agent behavior before integrating it into eShopLite.

Recommended doc:

```text
/docs/26 06 16 NET Agentic Modernization/devui-agent-validation-notes.md
```

Content should include:

1. What DevUI is used for in this repo:
   - testing agents,
   - iterating prompts,
   - inspecting tool calls,
   - checking workflow behavior,
   - viewing OpenTelemetry traces during development.
2. Why DevUI is optional:
   - it is useful for development,
   - it is not production,
   - it is not a main audience takeaway for this session.
3. Which agent scenarios can be validated with DevUI:
   - Store Intelligence Agent,
   - A2A Orchestrator Agent,
   - Observability Agent,
   - local NeMo-style agent adapter if applicable.
4. How DevUI output is moved back into the eShopLite app scenario:
   - validated instructions,
   - validated tool contracts,
   - test prompts,
   - expected responses.
5. A clear note that the live session should not spend time explaining DevUI unless there is a short backup/demo appendix.

## 12.8 Demo flow

1. Start eShopLite with the A2A scenario.
2. Show the agents in the Aspire dashboard.
3. Ask the demo question.
4. Show each agent contributing one part of the answer.
5. Show the orchestrator response.
6. Explain where remote Foundry agents or a local NeMo agent could fit.

## 12.9 Acceptance criteria

1. The scenario demonstrates agent specialization.
2. The final answer includes product, operational, and business context.
3. The agents do not perform unsafe write actions by default.
4. The docs clearly explain local and remote options.
5. The demo can be completed in under 5 minutes if run live, or shown as an architecture walkthrough if time is short.

---

# 13. Slide-only section: Deploy with Aspire

## 13.1 Story

Deployment should not be another demo in this session. It should be a slide-based closing section that connects the previous AI scenarios to the deployment options already provided by Aspire.

Core line:

> We are not changing the app architecture for each deployment target. Aspire gives us a deployment model from the same AppHost.

## 13.2 Required slide content

Add the following docs under the session folder:

```text
/docs/26 06 16 NET Agentic Modernization/slides/07-deploy-with-aspire.md
```

The slide content should cover:

1. One AppHost model.
2. Local development with Aspire.
3. `aspire publish` for deployment artifacts.
4. `aspire deploy` for direct deployment workflows.
5. Azure Container Apps.
6. Azure App Service.
7. Kubernetes / AKS.
8. Docker Compose.
9. Custom CI/CD pipelines.
10. Where Foundry hosted agents can fit.

## 13.3 Suggested slide diagram

```text
One AppHost model
        |
        v
Local dev with Aspire
        |
        +--> aspire publish
        |       +--> Docker Compose
        |       +--> Kubernetes / Helm
        |       +--> Bicep
        |
        +--> aspire deploy
                +--> Azure Container Apps
                +--> Azure App Service
                +--> Kubernetes / AKS
                +--> Custom deployment pipelines
```

## 13.4 Speaker notes

Suggested speaker script:

> This session is not about becoming a deployment expert in the last five minutes. The important point is that the same modernization foundation we used for local development and AI demos also gives us a deployment story. Aspire can publish artifacts or deploy directly, and the app model remains the mental model. That matters because once we add AI capabilities, we still need a practical way to move them from local demos to real environments.

---

# 14. Required session documentation folder

Add this folder:

```text
/docs/26 06 16 NET Agentic Modernization/
```

Required files:

```text
README.md
session-abstract.md
session-flow.md
speaker-script.md
demo-runbook.md
demo-prompts.md
slides-outline.md
top-5-pain-points.md
scenario-map.md
hosted-agents-evaluation.md
agent-framework-positioning.md
devui-agent-validation-notes.md
deploy-with-aspire-slide-notes.md
references.md
```

Notes:

- `hosted-agents-evaluation.md` must evaluate which agent scenarios can be exposed/deployed as Microsoft Foundry Hosted Agents through Aspire.
- `agent-framework-positioning.md` must explain Microsoft Agent Framework as the app-code layer for agents and workflows.
- `devui-agent-validation-notes.md` must be optional/developer-focused and explicitly say that DevUI is not production and not part of the main pitch.
- Do not add an Aspire MCP/coding-agent document to this session folder.

# 15. Session content

## 15.1 Session title

Recommended final title:

```text
App Modernization Done. Now Let's Make It Smarter.
```

Alternative title for repo docs:

```text
.NET Agentic Modernization with eShopLite
```

## 15.2 Session abstract

Modernizing a .NET application is a great milestone: the app runs on a current runtime, uses modern cloud patterns, has telemetry, and can be deployed with a repeatable model. But then comes the next question: now what?

In this session, we use eShopLite to show how a modernized .NET app can become smarter without a rewrite. We will add AI to specific parts of the app: observability, product discovery, business reporting, MCP tools, and agent-to-agent workflows. We will use Aspire as the local orchestration and deployment foundation, and we will connect the story to Microsoft Foundry, Foundry Local, MCP, and A2A.

## 15.3 Audience

Primary audience:

- Enterprise .NET developers.
- App modernization teams.
- Cloud architects.
- Developer advocates.
- Technical leads evaluating AI features after migration.

Secondary audience:

- Beginners learning where AI fits in real apps.
- Developers who have tried chat demos but want practical application scenarios.

## 15.4 Prerequisites

Attendees should understand:

- Basic .NET application structure.
- Basic API/web app concepts.
- Why observability matters.
- Basic AI concepts such as prompts, embeddings, and tool calling.

They do not need deep knowledge of MCP, A2A, or Aspire deployment internals.

---

# 16. Proposed 30-minute timing

| Time | Section | Type |
|---:|---|---|
| 0:00–2:00 | Opening: modernization is the foundation | Slides |
| 2:00–5:00 | eShopLite baseline with Aspire | Demo |
| 5:00–10:00 | Scenario 13: Observability Assistant | Demo |
| 10:00–14:00 | Scenario 14: Product Discovery Copilot | Demo |
| 14:00–18:00 | Scenario 15: Store Intelligence Report | Demo |
| 18:00–22:00 | Scenario 16: MCP Store Operations Tools | Demo |
| 22:00–25:00 | Scenario 17: A2A Store Operations Network | Demo or architecture walkthrough |
| 25:00–27:00 | Deploy with Aspire | Slides |
| 27:00–29:00 | Top 5 pain points | Slides |
| 29:00–30:00 | Closing | Slides |

If time gets tight, skip live A2A and show it as an architecture walkthrough.

---


Timing note:

- Do not allocate time to Aspire MCP or AI coding agents.
- DevUI is backup/appendix only.
- Hosted Agent evaluation is mentioned as a cloud-ready extension during the A2A section, not as a separate live demo.

# 17. Speaker script

Add this file:

```text
/docs/26 06 16 NET Agentic Modernization/speaker-script.md
```

## 17.1 Opening

> We spend a lot of time talking about modernization: upgrade the runtime, move to the cloud, improve deployment, add telemetry, make the app easier to operate. That is all important. But modernization is not the finish line. It is the moment where we finally have a foundation that AI can use.

> Today I want to show a simple idea: after the app is modernized, we can add AI to very specific parts of the application. Not a giant chatbot. Not a rewrite. Just practical AI features where the app already has useful signals.

## 17.2 Baseline

> The demo app is eShopLite. It is a modern .NET eCommerce sample, orchestrated with Aspire, with different scenarios for search, AI, MCP, agents, and deployment. For this session, I updated the scenarios to the latest .NET 10, latest packages, and latest Aspire flows.

> The important thing is that the app already has structure. It has services. It has logs. It has traces. It has APIs. It has product data. That is what makes the AI features useful.

## 17.3 Observability Assistant

> First pain point: logs. Modern apps generate a lot of telemetry, but humans are still expected to read everything manually. Here we add a local AI service using Foundry Local. The app can summarize what happened, group failures by service, and suggest what to check next.

> The key idea is not that AI replaces observability. The key idea is that AI sits on top of observability.

## 17.4 Product Discovery

> Next, let’s move to the user experience. Search is one of the best places to add AI because users already understand it. Instead of forcing users to know exact keywords, we let them express intent.

> This is not a random chatbot. It is grounded in the catalog and the search results.

## 17.5 Store Intelligence

> Modern apps produce a lot of business signals: searches, failed searches, product views, errors, and usage patterns. But those signals are usually spread across dashboards and databases. Here we generate a store intelligence report that combines business and operational context.

> This is where the app starts explaining what happened, not just storing what happened.

## 17.6 MCP

> Once the app has useful capabilities, we can expose them as tools. That is where MCP fits. MCP lets agents discover and use specific app capabilities in a controlled way.

> This is different from giving an agent unrestricted access. We expose a small set of tools: search catalog, get product details, get failed searches, get recent errors, and generate a store report.

## 17.7 A2A

> MCP lets an agent use your app. A2A lets agents collaborate around your app. In real enterprise workflows, one agent is usually not enough. You may need a catalog agent, an observability agent, and a business insights agent working together.

> This is also where we can connect to remote agents in Foundry, or to a local agent built with another framework such as NeMo, using an A2A pattern.

## 17.8 Deploy with Aspire

> The last piece is deployment. I will keep this slide-based because there is another session that goes deeper into Aspire. The important message is that the same AppHost model can support local development, publish flows, deploy flows, Azure targets, Kubernetes, Docker Compose, and custom pipelines.

## 17.9 Closing

> Modernization gave us the foundation. AI gives us the next layer of value. The path is not to rebuild everything. The path is to look at the modernized app and ask: where do we already have signals, APIs, and workflows that AI can make better?

---

# 18. Top 5 app pain points for closing slides

Add this file:

```text
/docs/26 06 16 NET Agentic Modernization/top-5-pain-points.md
```

## Pain point 1: We have logs, but nobody has time to read them

Modern apps generate more telemetry than teams can manually inspect.

AI opportunity:

- Log summarization.
- Trace explanation.
- Incident timelines.
- Root-cause hints.
- Suggested next actions.

Slide line:

> Observability without understanding is just more data.

## Pain point 2: Users do not search like databases think

Users describe intent, context, constraints, and goals. Traditional search often expects exact keywords.

AI opportunity:

- Semantic search.
- Hybrid search.
- Natural-language product discovery.
- Result explanations.
- Personalized recommendations.

Slide line:

> AI helps the app understand intent, not just keywords.

## Pain point 3: Enterprise apps know a lot, but explain very little

Apps store events, orders, searches, failures, and user actions, but business users still depend on manual reporting.

AI opportunity:

- Daily summaries.
- Business insight reports.
- Failed search analysis.
- Catalog improvement suggestions.
- Operational impact summaries.

Slide line:

> The next feature may not be another screen. It may be an explanation.

## Pain point 4: Every integration becomes custom glue

Modern apps have APIs, but agents need safe, discoverable capabilities.

AI opportunity:

- MCP tools.
- Agent-safe APIs.
- Capability discovery.
- Controlled tool access.
- Read-only operational tools.

Slide line:

> MCP turns app capabilities into agent-ready tools.

## Pain point 5: One agent is not enough for real enterprise workflows

Real workflows cross systems, teams, domains, and responsibilities.

AI opportunity:

- A2A.
- Specialized agents.
- Local plus cloud agents.
- Foundry hosted agents.
- External agent framework integration.

Slide line:

> The future is not one giant agent. It is specialized agents collaborating around modern apps.

---

# 19. Shared implementation guidance

## 19.1 Common architecture principles

1. Keep scenarios independent.
2. Reuse existing eShopLite service boundaries when practical.
3. Keep AI calls behind interfaces.
4. Keep model/provider selection configurable.
5. Support local-first demos when possible.
6. Keep cloud configuration optional unless the scenario specifically requires it.
7. Use structured outputs for AI responses.
8. Add deterministic demo data to avoid live-demo surprises.
9. Never send secrets to AI prompts.
10. Document all required configuration values.

## 19.2 Recommended shared abstractions

```csharp
public interface IChatCompletionService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}

public interface IStructuredAiService
{
    Task<TResponse> GenerateAsync<TResponse>(string taskName, object input, CancellationToken cancellationToken = default);
}

public interface IDemoDataGenerator
{
    Task GenerateAsync(DemoDataProfile profile, CancellationToken cancellationToken = default);
}

public interface ITelemetrySnapshotProvider
{
    Task<TelemetrySnapshot> GetSnapshotAsync(TimeSpan window, CancellationToken cancellationToken = default);
}
```

## 19.3 Configuration pattern

Each scenario should support app settings similar to:

```json
{
  "AI": {
    "Provider": "FoundryLocal",
    "Model": "local-model-name",
    "Endpoint": "http://localhost:port",
    "UseStructuredOutput": true
  },
  "Demo": {
    "UseDeterministicData": true,
    "Seed": 20260616
  }
}
```

Provider values may include:

```text
FoundryLocal
AzureOpenAI
GitHubModels
Mock
```

The `Mock` provider should be available for tests and offline docs validation.

## 19.4 Testing requirements

Each scenario must include:

1. Build validation.
2. Unit tests for contracts and prompt assembly.
3. Tests for deterministic sample data generation.
4. Tests for structured output parsing.
5. Smoke test that starts the AppHost or validates DI without requiring external secrets.

Recommended commands:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
```

If scenario-specific smoke tests are available:

```powershell
dotnet run --project src/eShopLite.AppHost -- --smoke-test
```

## 19.5 README requirements for each scenario

Each scenario README must include:

1. What this scenario demonstrates.
2. Architecture diagram or Mermaid diagram.
3. Prerequisites.
4. Required configuration.
5. How to run locally.
6. How to run the demo.
7. Expected output.
8. Troubleshooting.
9. Links back to the session docs folder.

---

# 20. Demo runbook

Add this file:

```text
/docs/26 06 16 NET Agentic Modernization/demo-runbook.md
```

## 20.1 Pre-demo checklist

1. Clone repo.
2. Confirm .NET 10 SDK is installed.
3. Confirm Aspire CLI is installed.
4. Confirm Docker Desktop or compatible container runtime is running if needed.
5. Confirm Foundry Local is installed and a supported model is available for Scenario 13.
6. Confirm no old local ports are in use.
7. Run build and tests.
8. Run each scenario once before the session.
9. Capture fallback screenshots for each demo.
10. Prepare a browser profile with zoom level set for streaming.

## 20.2 Scenario 13 checklist

1. Start AppHost.
2. Generate demo failure.
3. Open Aspire dashboard.
4. Open Observability Assistant page.
5. Generate summary.
6. Confirm trace IDs and grouped services are visible.

## 20.3 Scenario 14 checklist

1. Start AppHost.
2. Search with keyword.
3. Search with natural language.
4. Show product explanations.
5. Show grounded product fields.

## 20.4 Scenario 15 checklist

1. Generate demo activity.
2. Show raw signals.
3. Generate report.
4. Show recommended actions.

## 20.5 Scenario 16 checklist

1. Start MCP server.
2. Connect MCP client.
3. Call `search_catalog`.
4. Call `get_recent_errors`.
5. Call `get_store_intelligence_report`.

## 20.6 Scenario 17 checklist

- Hosted Agent evaluation doc exists.
- Agent Framework positioning is documented.
- DevUI validation notes are optional and developer-focused.
- Live demo can run locally without requiring cloud Hosted Agents.
- If a Hosted Agent path is implemented, it is validated against the current Aspire package version.


1. Start agents.
2. Ask demo question.
3. Show each agent contribution.
4. Show final orchestrated response.
5. Mention Foundry hosted agent and NeMo local options.

---

# 21. Slide content outline

Add this file:

```text
/docs/26 06 16 NET Agentic Modernization/slide-content.md
```

## Slide 0: Title

**App Modernization Done. Now Let's Make It Smarter.**

Subtitle:

**Adding AI to a modern .NET app with eShopLite, Aspire, Foundry, MCP, and A2A**

## Slide 1: The modernization moment

- The app is on modern .NET.
- The app is observable.
- The app is orchestrated.
- The app can be deployed.
- The app has clean service boundaries.

Speaker line:

> Now what?

## Slide 2: The AI ladder

```text
Level 1: Understand the app
Level 2: Improve the user experience
Level 3: Explain business signals
Level 4: Expose app capabilities as tools
Level 5: Coordinate specialized agents
Level 6: Deploy through the same app model
```

## Slide 3: Demo map

```text
eShopLite + Aspire
  -> Observability Assistant
  -> Product Discovery Copilot
  -> Store Intelligence Report
  -> MCP Store Tools
  -> A2A Store Operations Network
  -> Deploy with Aspire
```

## Slide 4: Observability Assistant

Main message:

> Logs are useful. Summaries are usable.

## Slide 5: Product Discovery

Main message:

> Search evolves from keywords to intent.

## Slide 6: Store Intelligence

Main message:

> The app can explain what happened today.

## Slide 7: MCP

Main message:

> App capabilities become agent-ready tools.

## Slide 8: A2A

Main message:

> Specialized agents collaborate around the app.

## Slide 9: Deploy with Aspire

Main message:

> Same app model. Multiple deployment paths.

## Slide 10: Top 5 pain points

- Logs nobody reads.
- Search that misses intent.
- Data without explanations.
- Integration glue everywhere.
- Workflows bigger than one agent.

## Slide 11: Closing

> Modernization is not the finish line. It is the foundation that makes AI useful.

---

# 22. Quality gates

Before the PR is considered complete:

## 22.1 Repository quality gates

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
```

## 22.2 Scenario quality gates

For each new scenario:

1. README exists.
2. Scenario runs locally.
3. AppHost starts.
4. Demo data can be generated.
5. AI provider can be configured.
6. Mock provider works for tests.
7. No secrets are committed.
8. Demo script exists.
9. Troubleshooting doc exists.
10. Scenario can be explained in less than 5 minutes.

## 22.3 Documentation quality gates

The session docs folder must include:

1. `README.md`
2. `session-abstract.md`
3. `session-outline.md`
4. `speaker-script.md`
5. `demo-runbook.md`
6. `slide-content.md`
7. `top-5-pain-points.md`
8. One demo doc per scenario.
9. One architecture doc per major session section.
10. `references.md`

---

# 23. Suggested GitHub issues

## Issue 1: Add session documentation folder

Create `/docs/26 06 16 NET Agentic Modernization/` with the session outline, speaker script, slide content, demo runbook, and references.

## Issue 2: Add Scenario 13 - Observability Assistant with Foundry Local

Implement local AI summarization of logs/traces using Foundry Local and Aspire orchestration.

## Issue 3: Add Scenario 14 - Product Discovery Copilot

Implement natural-language product discovery grounded in the eShopLite catalog and search results.

## Issue 4: Add Scenario 15 - Store Intelligence Report

Implement a daily store summary that combines business and operational signals.

## Issue 5: Add Scenario 16 - MCP Store Operations Tools

Expose safe eShopLite capabilities as MCP tools.

## Issue 6: Add Scenario 17 - A2A Store Operations Network

Implement or document a multi-agent flow with Catalog, Observability, Business Insights, and Orchestrator agents.

## Issue 7: Add slide-only Deploy with Aspire section

Document the deployment story using current Aspire deployment options.

## Issue 8: Add top 5 pain points closing slides

Add the closing pain point slides and speaker notes.

---

## Issue 9: Add Hosted Agents evaluation docs for agent scenarios

Create:

```text
/docs/26 06 16 NET Agentic Modernization/hosted-agents-evaluation.md
/scenarios/17-A2AStoreOperationsNetwork/docs/foundry-hosted-agent-evaluation.md
```

Acceptance criteria:

- Documents which agents can remain local.
- Documents which agents could become Foundry Hosted Agents.
- Documents required configuration and identity assumptions.
- Validates hosted-agent APIs against the current Aspire package version.
- Explicitly keeps Hosted Agents as an extension/evaluation path, not the main demo.

## Issue 10: Add Agent Framework and DevUI positioning docs

Create:

```text
/docs/26 06 16 NET Agentic Modernization/agent-framework-positioning.md
/docs/26 06 16 NET Agentic Modernization/devui-agent-validation-notes.md
```

Acceptance criteria:

- Agent Framework is positioned as the app-code layer for agents and workflows.
- DevUI is described as optional, development-only, and not production.
- The docs do not include Aspire MCP or AI coding-agent flows as part of the session pitch.


# 24. References

- eShopLite: https://github.com/Azure-Samples/eShopLite
- Aspire 13.4: https://aspire.dev/whats-new/aspire-13-4/
- Aspire 13.4 Foundry Hosted Agents section: https://aspire.dev/whats-new/aspire-13-4/#foundry-hosted-agent-commands-and-fixes
- Aspire deployment docs: https://aspire.dev/deployment/
- Microsoft Agent Framework overview: https://learn.microsoft.com/en-us/agent-framework/overview/
- Microsoft Agent Framework A2A integration: https://learn.microsoft.com/en-us/agent-framework/integrations/a2a
- Microsoft Agent Framework DevUI: https://learn.microsoft.com/en-us/agent-framework/devui/
- MAF + A2A + NVIDIA NeMo Agents sample: https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents

# 25. Final recommended story

Use this as the session's repeated framing:

```text
Because we modernized the app,
we now have telemetry.

Because we have telemetry,
AI can explain the app.

Because we have product data and search,
AI can improve user experience.

Because we have business signals,
AI can generate insights.

Because we have clean APIs,
AI can use the app through MCP.

Because agents need to collaborate,
A2A connects specialized agents.

Because we have Aspire,
we can move the same model from local development to deployment.
```

Final closing line:

> Modernization is not the finish line. It is the foundation that makes AI useful.


---

## Addendum: Branching and scenario derivation rules

All implementation work for this PRD must be done in a new branch named:

```bash
git checkout -b bruno-NETAgenticModernizationSession
```

New scenarios must start from `01 - Semantic Search` by default. A scenario may start from a different existing scenario only when it requires scenario-specific infrastructure that is already present elsewhere, such as MCP, A2A, or deployment-specific assets. Each scenario `README.md` must state which existing scenario it was derived from and why.
