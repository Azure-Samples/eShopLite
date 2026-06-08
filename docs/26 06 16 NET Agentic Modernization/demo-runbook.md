# Demo Runbook - .NET Agentic Modernization

## Purpose

This runbook helps rehearse and run the demos for the session:

**App Modernization Done. Now Let's Make It Smarter**

It is optimized for reliability. Each demo includes a goal, setup, commands, expected output, and fallback.

## Branch

Use this branch for all work:

```bash
git checkout -b bruno-NETAgenticModernizationSession
```

## General prerequisites

Validate these before rehearsal:

```bash
dotnet --version
aspire --version
git status
```

Recommended baseline:

- Latest .NET 10 SDK used by the repository.
- Latest Aspire packages already updated in the repository.
- Latest NuGet packages already updated in the repository.
- eShopLite main solution builds locally.
- Foundry Local installed/configured if the Observability Assistant uses a local model.
- Any cloud AI endpoints configured only when needed.
- No secrets committed to the repo.

## General rehearsal checklist

Before every rehearsal:

```bash
git status
dotnet restore
dotnet build
```

Then run the target scenario from its own folder.

Each new scenario README should include its exact command. Prefer commands that are easy to copy/paste.

## Demo reliability rules

- Use deterministic sample data where possible.
- Have saved outputs for every AI-generated answer.
- Never depend on a cloud deployment during the live session.
- Keep Hosted Agents as an evaluation/extension path unless the setup is already reliable.
- Keep DevUI optional.
- Do not include Aspire MCP / AI coding-agent workflow in the pitch.

## Demo 0 - Baseline eShopLite

### Goal

Show the modernized app foundation.

### Show

- App running locally.
- Aspire dashboard.
- Resources.
- Logs/traces.
- Product catalog.
- Search.

### Speaker message

> This is not where the modernization story ends. This is where the smarter app story starts.

### Expected outcome

Audience understands that eShopLite is already modernized and ready to be extended.

### Fallback

If the app does not start:

- show screenshots,
- show the AppHost file,
- show the scenario map,
- continue with slides and saved demo outputs.

---

## Demo 1 - Product Discovery

### Scenario folder

```text
/scenarios/14-ProductDiscoveryCopilot
```

### Format

**Code walkthrough — no app run.** Do not launch this scenario. Read the code and explain
how data is grounded. Full script: `demo-14scenario.md`.

### Goal

Explain intent-based product discovery and grounded explanations from the code.

### Setup

- Open `scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs`.
- No run, no secrets, no containers required.

### File to show

```text
scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs
```

### Walkthrough steps

1. `InitMemoryContextAsync` — every catalog product is embedded into an in-memory vector
   store at startup; show the system prompt (on-catalog/honesty guardrail).
2. `Search` — embed the shopper's question and vector-search the catalog (top 3).
3. Show the `Score > 0.3` gate — the no-match / no-invented-products guardrail.
4. Show the `Found Products` block + grounding prompt built only from matched products, sent
   to the chat model with the system prompt.
5. Land the point: the model only ever sees the products the vector search returned.

### Key takeaway

- Grounding is built from the catalog (embeddings at startup), not the model's training data.
- The `> 0.3` gate + system prompt prevent invented products.
- This uses a **cloud** model — it sets up the contrast for Demo 2, which runs AI locally.

### Fallback

Code reading — nothing to run or recover. If time is short, show only `Search`.

---

## Demo 2 - Observability Assistant (local AI)

### Scenario folder

```text
/scenarios/13-ObservabilityAssistantFoundryLocal
```

### Detailed playbook

Use the full presenter script, prompts, expected outputs, fallback, and code walkthrough from:

`docs/26 06 16 NET Agentic Modernization/demo-13scenario.md`

### Goal

Run local observability flow with three Aspire services (`products`, `store`, `observabilityassistant`) and produce developer next actions — with the model running **locally** via Foundry Local.

### Required presenter callout

- This is **local AI**: the analysis model runs on the developer's machine via Foundry Local; no telemetry leaves the box.
- The Store page calls `observabilityassistant` backend.
- The backend analyzes **real ingested events** (not synthetic `BuildLogs`) and Store displays findings.
- Foundry Local model is selected via config: `FoundryLocal:SelectedModel` + `FoundryLocal:Models` catalog.
- **Inject Search Failure** is **disabled by default** — enable it before searching to intentionally produce telemetry errors.
- Run the same narrative across four windows: **5 / 10 / 15 / 30 minutes**.

### Practical presenter flow

1. Open Store **Search** and **enable** `Inject Search Failure`.
2. Run several searches (normal + semantic) to generate telemetry errors/noisy traces.
3. Run assistant analysis windows in order: **5**, **10**, **15**, **30** minutes.

### Key takeaway

- Demo 1 used a cloud model for users; here the same modernized app runs AI **locally** for operations.
- Foundry Local is a natural fit for sensitive, high-volume logs and traces.

### Fallback

If the local AI service fails:

- show a saved output sample,
- explain the expected input/output,
- show the raw logs and how the summary would help,
- move to Store Intelligence.

---

## Demo 3 - Store Intelligence Report

### Scenario folder

```text
/scenarios/15-StoreIntelligenceReport
```

### Goal

Generate a business summary from app signals.

### Setup

- Use deterministic search and product activity.
- Include failed searches and product gaps.
- Include optional operational issue summary.

### Commands

```bash
cd scenarios/15-StoreIntelligenceReport
dotnet run --project src/eShopLite.AppHost
```

### Demo steps

1. Show raw store signals.
2. Ask for a report.
3. Show business summary.
4. Explain how it maps to app data.

### Prompt

```text
Create today's store intelligence report. Include top searches, failed searches, product opportunities, operational issues that may affect customers, and recommended next actions.
```

### Expected result

- Executive summary.
- Top searches.
- Failed searches.
- Product opportunities.
- Operational issues.
- Recommended next actions.

### Fallback

Use:

```text
/scenarios/15-StoreIntelligenceReport/demo-assets/store-intelligence-sample.md
```

---

## Demo 4 - MCP Store Tools

### Scenario folder

```text
/scenarios/16-MCPStoreOperationsTools
```

### Goal

Expose app capabilities as tools.

### Setup

- Prefer Scenario 01 as baseline.
- Reuse MCP patterns from the existing MCP scenario if needed.
- Keep tool list small and app-specific.

### Suggested tools

```text
search_catalog
get_product_details
get_failed_searches
get_store_summary
get_recent_operational_summary
generate_store_intelligence_report
```

### Commands

```bash
cd scenarios/16-MCPStoreOperationsTools
dotnet run --project src/eShopLite.AppHost
```

### Demo steps

1. Show the tools.
2. Run a prompt that requires multiple tools.
3. Show the grounded result.
4. Explain why this is safer than a generic chatbot.

### Prompt

```text
Customers are searching for travel products. Use the store tools to find matching products, identify failed searches, and summarize product gaps.
```

### Expected result

- Tool calls use app capabilities.
- Answer is grounded in tool outputs.
- No direct database or uncontrolled app access is required.

### Fallback

Show:

```text
/scenarios/16-MCPStoreOperationsTools/docs/tool-contracts.md
/scenarios/16-MCPStoreOperationsTools/demo-assets/mcp-tool-flow-sample.md
```

---

## Demo 5 - A2A Store Operations Network

### Scenario folder

```text
/scenarios/17-A2AStoreOperationsNetwork
```

### Goal

Show specialized agents collaborating.

### Setup

- Prefer Scenario 01 as baseline.
- Reuse A2A patterns from the existing A2A scenario if needed.
- Optionally reference local agent patterns from:
  https://github.com/elbruno/MAF-A2A-NVIDIA-NemoAgents

### Agents

```text
Orchestrator Agent
Catalog Agent
Observability Agent
Business Insights Agent
```

### Commands

```bash
cd scenarios/17-A2AStoreOperationsNetwork
dotnet run --project src/eShopLite.AppHost
```

### Demo steps

1. Ask investigation question.
2. Show orchestrator plan.
3. Show calls to specialized agents.
4. Show final action plan.
5. Mention Hosted Agents as an evaluation path.

### Prompt

```text
Customers are searching for travel products but not converting. Investigate the likely reasons and suggest actions.
```

### Expected result

```text
- Catalog findings.
- Business findings.
- Observability findings.
- Combined recommendation.
```

### Fallback

Show:

```text
/scenarios/17-A2AStoreOperationsNetwork/docs/architecture.md
/scenarios/17-A2AStoreOperationsNetwork/demo-assets/a2a-final-response-sample.md
```

## Deploy section

This is slide-only.

Do not run a deployment demo.

Mention:

- `aspire publish`
- `aspire deploy`
- Azure Container Apps
- Azure App Service
- Kubernetes / AKS
- Docker Compose
- Custom pipelines

Use:

```text
deploy-with-aspire-slide-notes.md
```

## Final rehearsal checklist

- All demos have saved sample outputs.
- All prompts are copied into `demo-prompts.md`.
- All scenario READMEs include source baseline.
- All scenario READMEs include run commands.
- Hosted Agents are not a live-demo dependency.
- DevUI is optional and development-only.
- Aspire MCP / AI coding-agent pitch is not included.
