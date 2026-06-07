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

## Demo 1 - Observability Assistant

### Scenario folder

```text
/scenarios/13-ObservabilityAssistantFoundryLocal
```

### Detailed playbook

Use the full presenter script, prompts, expected outputs, fallback, and code walkthrough from:

`docs/26 06 16 NET Agentic Modernization/demo-13scenario.md`

### Goal

Run local observability flow with three Aspire services (`products`, `store`, `observabilityassistant`) and produce developer next actions.

### Required presenter callout

- The Store page calls `observabilityassistant` backend.
- The backend analyzes **real ingested events** (not synthetic `BuildLogs`) and Store displays findings.
- Foundry Local model is selected via config: `FoundryLocal:SelectedModel` + `FoundryLocal:Models` catalog.
- Search starts with **Inject Search Failure** enabled by default to intentionally produce telemetry errors.
- Run the same narrative across four windows: **5 / 10 / 15 / 30 minutes**.

### Practical presenter flow

1. Open Store **Search** and keep `Inject Search Failure` enabled.
2. Run several searches (normal + semantic) to generate telemetry errors/noisy traces.
3. Run assistant analysis windows in order: **5**, **10**, **15**, **30** minutes.

---

## Demo 2 - Product Discovery

### Scenario folder

```text
/scenarios/14-ProductDiscoveryCopilot
```

### Goal

Show intent-based product discovery.

### Setup

- Start from Scenario 01.
- Ensure catalog data is stable.
- Ensure search/indexing is ready.
- Prepare 3 deterministic product-discovery prompts.

### Commands

```bash
cd scenarios/14-ProductDiscoveryCopilot
dotnet run --project src/eShopLite.AppHost
```

### Demo steps

1. Search with a normal keyword.
2. Search with a natural-language intent.
3. Ask for explanation.
4. Show grounded results.

### Prompt

```text
Find products that are good for walking all day and explain why each result matches.
```

### Expected result

- Products are returned from catalog data.
- Explanation references product attributes.
- No unsupported claims.

### Fallback

Use a prepared product list and explanation from:

```text
/scenarios/14-ProductDiscoveryCopilot/demo-assets/product-discovery-sample.md
```

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
