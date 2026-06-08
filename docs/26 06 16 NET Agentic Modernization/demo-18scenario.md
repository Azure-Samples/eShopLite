# Demo 6 (Bonus) Playbook - Scenario 18 MAF Dev UI

> **Format: live walkthrough (bonus / optional).** This scenario hosts **Microsoft Agent Framework
> (MAF)** agents inside the Store and exposes the **Agent Framework Dev UI** at `/devui` for
> inspecting and running agents, plus a multi-agent **checkout orchestration** workflow. It is a
> **bonus** demo — not part of the core slide arc (Demos 1-5). Use it if time allows or for deeper
> Q&A on the Agent Framework.

## Objective and presenter story

**Objective:** show the **developer-facing** side of agents: a built-in Dev UI to inspect, test, and
run MAF agents, and a workflow that orchestrates several agents to complete a real task (checkout).

**Story line:**

1. Demos 4-5 showed tools (MCP) and agent-to-agent collaboration (A2A).
2. MAF gives a **first-class agent runtime** in .NET, with a **Dev UI** to inspect and run agents.
3. The Store registers eShopLite agents (triage, stock, discount) and a **checkout orchestrator**.
4. Open `/devui` to see the agents, send them messages, and watch a workflow run.
5. This proves you can **build, debug, and operate** agents with first-class .NET tooling.

## Where this sits in the arc

- **Demos 1-3:** AI inside the app (users, ops, business).
- **Demo 4 (MCP):** the store's abilities as callable tools.
- **Demo 5 (A2A):** specialist agents collaborating.
- **Demo 6 (MAF Dev UI, this bonus):** the **developer experience** for building/running agents.

## What's inside

| Piece | Role | Where |
|---|---|---|
| MAF agents (triage / stock / discount) | Focused agents registered in the Store | `AgentServices/` (Triage, Stock, Discount) |
| `AgentCheckoutOrchestrator` | Orchestrates agents to complete a checkout | `AgentServices/Checkout/AgentCheckoutOrchestrator.cs` |
| **Dev UI** | Inspect + run agents in the browser | mapped at `/devui` (Store, Development only) |
| Foundry-hosted agents | `AddeShopLiteFoundryAgents()` | `Store/Program.cs` + `microsoftfoundryproject` connection string |

## Current state

- Scenario 18 is the **MAF Dev UI** scenario (formerly numbered 14). Its AppHost lives at the
  **scenario root** (no `src/` subfolder); the solution is `eShopLite-18-MAFDevUI.slnx`.
- Resources: `sql` + `products` + `store` (the Store hosts the MAF agents and the Dev UI).
- The Store nav has a **DevUI** link that opens `/devui` in a new tab (Development environment only).
- The agents are **Foundry-hosted** via `AddeShopLiteFoundryAgents()`, so this scenario also needs an
  **Azure AI Foundry project endpoint** (`microsoftfoundryproject` connection string) in addition to
  the usual Azure OpenAI parameters.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source at: `D:\azure-samples\eShopLite\scenarios\18-MAFDevUI`
- The four Azure OpenAI parameters (same as the other scenarios):
  - `Parameters:AzureOpenAIEndpoint`, `Parameters:AzureOpenAIApiKey`,
    `Parameters:AzureOpenAIDeploymentName`, `Parameters:AzureOpenAIEmbeddingsDeploymentName`
- An **Azure AI Foundry project** connection string: `ConnectionStrings:microsoftfoundryproject`
  (required for the Foundry-hosted agents). Without it the Dev UI still loads but the agents that
  depend on Foundry will not run.

Quick validation:

```pwsh
dotnet --version
aspire --version
docker --version
```

## Run instructions (preferred: `aspire start`)

### Preferred flow

```pwsh
cd D:\azure-samples\eShopLite\scenarios\18-MAFDevUI\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:

- `sql`, `products`, and `store` resources are `Running`.
- Open the Dashboard URL shown by Aspire and pin the `store` endpoint in a browser tab.
- The Dev UI is only mapped in **Development** — confirm the Store runs in the Development environment.

### Alternative (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\18-MAFDevUI
dotnet restore
dotnet build eShopLite-18-MAFDevUI.slnx
cd .\eShopAppHost
aspire start --non-interactive
```

> Note: this scenario's AppHost is at the **scenario root** (`18-MAFDevUI\eShopAppHost`), not under
> a `src/` folder. Always start from there.

> ⚠️ **DevUI reachability caveat (validated 2026-06):** under `aspire start` the Store runs in the
> Development environment, but `/devui` may return **404** because Blazor interactive routing
> (`MapRazorComponents`) can intercept the path before the DevUI middleware. If `/devui` 404s, run the
> Store standalone with `dotnet run` (Development) from `scenarios\18-MAFDevUI\Store`, or use the
> **DevUI** nav link, and confirm Foundry-hosted agents are configured. This is the **bonus** demo —
> if DevUI doesn't come up live, fall back to the plan-B screenshots/GIF (`screenshots\scenario18-*`)
> and the code walkthrough of `AgentServices` + `Store/Program.cs` (`AddDevUI()` / `MapDevUI()`).

## Step-by-step live demo script

1. **Set context (20s):**
   "Demos 4 and 5 were about agents at runtime. This is the **developer experience**: a built-in
   Dev UI to inspect and run Microsoft Agent Framework agents in .NET."
2. **Open the Store:** from the Aspire Dashboard open the `store` endpoint.
3. **Open the Dev UI:** click **DevUI** in the left nav (opens `/devui` in a new tab).
4. **Browse the agents:** point at the registered eShopLite agents (triage, stock, discount). "These
   are first-class MAF agents registered in the Store via `AddeShopLiteFoundryAgents()`."
5. **Run an agent:** pick the stock or triage agent, send a message (e.g., a product question), and
   watch the response stream in the Dev UI. "No custom test harness — the Dev UI runs the agent for me."
6. **Show the checkout orchestration (the key move):** explain `AgentCheckoutOrchestrator` — it
   coordinates multiple agents to complete a checkout workflow. Point at the code while it runs.
7. **Tie it back:** "MCP was tools, A2A was agents-to-agents; MAF is the **runtime + tooling** that
   makes building and debugging those agents a first-class .NET experience."
8. **Close with value line (15s):**
   "Same modernized store. Now with a developer loop for agents — inspect, run, orchestrate — built
   into the framework."

## Expected output

```text
Store nav → DevUI (opens /devui)

Dev UI
  Agents:
    - triage
    - stock
    - discount
  [ select an agent → send a message → streamed response ]

Checkout orchestration
  AgentCheckoutOrchestrator coordinates agents to complete a checkout workflow.
```

## How the demo maps to MAF concepts

| What you see | MAF concept | Where in the code |
|---|---|---|
| The DevUI page with agents | Agent Framework Dev UI | `Store/Program.cs` (`AddDevUI()` / `MapDevUI()`) |
| Registered agents | MAF agents registered in DI | `Store/Program.cs` (`AddeShopLiteFoundryAgents()`) + `AgentServices/` |
| Checkout workflow | Multi-agent orchestration | `AgentServices/Checkout/AgentCheckoutOrchestrator.cs` |
| Streamed agent responses | MAF agent run | the Dev UI invoking the agents |

## Fallback plan

- If the **Foundry** endpoint is missing: the Dev UI page still loads — show the agent list and the
  orchestrator code, and explain the run path from the screenshots in `screenshots/`.
- If `/devui` is **not reachable**: confirm the Store is in the Development environment (Dev UI is
  Development-only), or present the screenshots.
- This is a **bonus** demo — if time is short, **skip it**. The session's core story (Demos 1-5) is
  complete without it.

## Key files to talk about

| File | Role | What to say |
|---|---|---|
| `scenarios/18-MAFDevUI/Store/Program.cs` | **Dev UI wiring** | `AddDevUI()` + `MapDevUI()` map the Agent Framework Dev UI at `/devui` (Development only). |
| `scenarios/18-MAFDevUI/AgentServices/AgentServicesExtensions.cs` | **Agent registration** | Registers MAF agents and tools in DI; follows the Agent Framework `AgentWebChat` pattern. |
| `scenarios/18-MAFDevUI/AgentServices/Checkout/AgentCheckoutOrchestrator.cs` | **Orchestration** | Coordinates agents to complete a checkout workflow. |
| `scenarios/18-MAFDevUI/Store/Components/Layout/NavMenu.razor` | **The entry point** | The DevUI nav link that opens `/devui`. |
| `scenarios/18-MAFDevUI/eShopAppHost/Program.cs` | **The host** | Registers `sql` + `products` + `store`; the Store hosts the agents and Dev UI. |

## Talking points / FAQ

- **"Is this required for the session?"** No — it's a **bonus**. The core arc is Demos 1-5.
- **"What is the Dev UI?"** A built-in browser tool from the Microsoft Agent Framework to inspect and
  run agents without writing a custom test harness.
- **"Why Foundry-hosted agents?"** This scenario demonstrates agents hosted via an Azure AI Foundry
  project; it needs the `microsoftfoundryproject` connection string in addition to Azure OpenAI.
- **"How does this relate to MCP/A2A?"** MCP = tools, A2A = agents-to-agents, MAF = the **runtime and
  developer tooling** for building those agents in .NET.

## Notes

- The AppHost is at the **scenario root** (`18-MAFDevUI\eShopAppHost`); the solution is
  `eShopLite-18-MAFDevUI.slnx`. Start `aspire start` from the scenario-root AppHost.
- The Dev UI is mapped **only in Development**; ensure the Store runs in that environment.
- This is the **bonus** demo. Previous: Demo 5 (A2A Store Operations Network).

## Reference links

- [.NET Aspire AI Agent Readiness](https://aspire.dev/whats-new/aspire-13-4/) — Aspire's integrated support for AI agents, observability, and deployment; the control plane for agent-driven applications.
- [Azure Foundry Agents Overview](https://learn.microsoft.com/en-us/azure/foundry/) — Enterprise agent deployment and orchestration; the hosting platform for the Foundry-hosted agents in this scenario.
- [Building AI Applications with .NET](https://learn.microsoft.com/en-us/dotnet/ai/) — Unified abstractions (`IChatClient`, `IEmbeddingGenerator`), agent patterns, and .NET tooling for AI-powered apps.
