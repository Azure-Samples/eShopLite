# Demo 5 Playbook - Scenario 17 A2A Store Operations Network

> **Format: fully live.** This scenario runs a small **Agent-to-Agent (A2A)** network. The Products
> API acts as a **Store Operations Orchestrator** that fans a request out to three specialist agents
> and aggregates their answers onto the product cards. The agents have been **remapped to store-ops
> roles** so the network tells the operations story.

## Objective and presenter story

**Objective:** show the next step beyond tools (Demo 4): **specialist agents that collaborate**. One
orchestrator coordinates several focused agents, each owning one concern, and composes a single
enriched answer.

**Story line:**

1. Demo 4 made the store's abilities callable as MCP **tools**.
2. A2A goes further: independent **agents**, each with its own skill, talk to one another.
3. The Products API is the **orchestrator** — for each product it asks three specialists in parallel.
4. The Store's Search page (A2A mode) shows the **aggregated** result: stock, promotions, insights per product.
5. This proves the store can be operated by a **network of cooperating agents**, not one monolith.

## Where this sits in the arc

- **Demo 4 (MCP Store Tools):** the store's abilities become callable **tools**.
- **Demo 5 (A2A Store Operations Network, this demo):** specialist **agents** collaborate via A2A.

## The remapped store-operations agent network

| Role | Agent | Owns | A2A skill / endpoint |
|---|---|---|---|
| **Orchestrator** | Products API (`A2AOrchestrationService`) | Fan-out + aggregation | calls all three specialists per product |
| **Specialist 1** | `CatalogAgent` (`catalog-agent`) | Catalog availability / stock | `check_catalog_availability` → `/api/inventory/check` |
| **Specialist 2** | `PromotionsAgent` (`promotions-agent`) | Promotions / offers | promotions skill |
| **Specialist 3** | `BusinessInsightsAgent` (`businessinsights-agent`) | Insights / reviews | `get_insights` → `/api/researcher/insights` |

> Names/roles were remapped to a store-ops vocabulary (Inventory→Catalog, Researcher→Business
> Insights; Promotions kept). Service-discovery labels were updated to match (`catalog-agent`,
> `businessinsights-agent`). The A2A behavior and the standalone agent projects are unchanged and
> runnable; both `eShopLite-A2A.slnx` and its tests build clean.

## Current state

- Scenario 17 is derived from **Scenario 10 (A2A Network)** and is **runnable**.
- Resources: `sql` + three agent projects (`catalog-agent`, `promotions-agent`,
  `businessinsights-agent`) + `products` (orchestrator) + `store`.
- The Store Search page has a **Search Type** dropdown including **A2A Search (Agent-to-Agent)**;
  in that mode product cards show **Stock / Promotions / Reviews** aggregated by the orchestrator.
- Verified: `eShopLite-A2A.slnx` builds clean after the remap; tests updated to the new class names.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source at: `D:\azure-samples\eShopLite\scenarios\17-A2AStoreOperationsNetwork`
- The Products service needs the four Azure OpenAI parameters (same as Scenario 01/14/15/16):
  - `Parameters:AzureOpenAIEndpoint`, `Parameters:AzureOpenAIApiKey`,
    `Parameters:AzureOpenAIDeploymentName`, `Parameters:AzureOpenAIEmbeddingsDeploymentName`

Quick validation:

```pwsh
dotnet --version
aspire --version
docker --version
```

## Run instructions (preferred: `aspire start`)

### Preferred flow

```pwsh
cd D:\azure-samples\eShopLite\scenarios\17-A2AStoreOperationsNetwork\src\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:

- `sql`, `catalog-agent`, `promotions-agent`, `businessinsights-agent`, `products`, and `store`
  resources are `Running`.
- Open the Dashboard URL shown by Aspire. Note the three agent resources — point at them later.
- Pin the `store` endpoint in a browser tab.

### Alternative (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\17-A2AStoreOperationsNetwork\src
dotnet restore
dotnet build
cd .\eShopAppHost
aspire start --non-interactive
```

> Note: always launch from **this scenario's** `eShopAppHost`.

## Step-by-step live demo script

1. **Set context (20s):**
   "Demo 4 gave us tools. Now we have a team of agents. The Products API is the orchestrator; three
   specialists own catalog availability, promotions, and business insights."
2. **Show the network in the Dashboard:** in the Aspire Dashboard point at the three agent resources
   — `catalog-agent`, `promotions-agent`, `businessinsights-agent` — plus `products` (the
   orchestrator). "Four cooperating services, not one monolith."
3. **Open the Store Search page:** open the `store` endpoint, click **Search**.
4. **Pick A2A mode:** in **Search Type**, select **A2A Search (Agent-to-Agent)**.
5. **Run a query:** type `winter camping` and click **Search**.
6. **Walk an enriched card (the key move):** each product card now shows three extra lines —
   **Stock**, **Promotions**, **Reviews**. "The orchestrator asked all three specialists in parallel
   for every product and merged their answers into one card."
7. **Tie each line to an agent:** Stock ← `CatalogAgent`; Promotions ← `PromotionsAgent`;
   Reviews/insights ← `BusinessInsightsAgent`. "Three independent agents, one composed result."
8. **Contrast with Standard/Semantic:** switch **Search Type** back to **Standard** and re-run — the
   enrichment lines disappear. "Same products; the A2A network is what adds the operational context."
9. **Close with value line (15s):**
   "This is the store operated as a network of agents. Add a new specialist — pricing, fraud,
   logistics — and the orchestrator composes it in. The store becomes a team."

## The ask (what A2A mode runs)

Selecting **A2A Search** and searching runs the orchestrator fan-out. Say it out loud:

```text
For each matching product, ask the catalog, promotions, and business-insights agents in parallel,
then merge their answers (stock, promotions, insights) onto the product card.
```

## Expected output

```text
Search Products
  Search Type: [ A2A Search (Agent-to-Agent) ]

Product cards (each):
  <image>
  <name>
  <description>
  Stock:       <from CatalogAgent>
  Promotions:  <from PromotionsAgent>
  Reviews:     <from BusinessInsightsAgent>
  Price: <price>
```

## How the demo maps to A2A concepts

| What you see | A2A concept | Where in the code |
|---|---|---|
| Three agent resources in the Dashboard | Independent agents, each its own service | `eShopAppHost/Program.cs` (`catalog-agent`, `promotions-agent`, `businessinsights-agent`) |
| A2A search merging three answers | Orchestrator fan-out + aggregation | `Products/Services/A2AOrchestrationService.cs` |
| Stock / Promotions / Reviews per card | Each specialist's contribution | `Products/Services/Agents/{CatalogAgent,PromotionsAgent,BusinessInsightsAgent}.cs` |
| Agent skills (`check_catalog_availability`, `get_insights`) | A2A `AgentSkill` definitions | the agent client classes |

## Fallback plan

- If **one agent** is slow/unavailable: the orchestrator still returns the other lines; narrate the
  resilience ("one specialist down, the network still answers").
- If **A2A mode** misbehaves: show **Standard**/**Semantic** search to prove the catalog works, then
  walk the A2A enrichment from the screenshots in `screenshots/`.
- If the **app** will not start: present the "Expected output" block, point at the three agents in the
  architecture, and continue to **Demo 6 (MAF Dev UI)** without retry loops.

## Key files to talk about

| File | Role | What to say |
|---|---|---|
| `scenarios/17-A2AStoreOperationsNetwork/src/Products/Services/A2AOrchestrationService.cs` | **The orchestrator** | Fans out to all three specialists per product and aggregates Stock/Promotions/Insights. The heart of the demo. |
| `scenarios/17-A2AStoreOperationsNetwork/src/Products/Services/Agents/CatalogAgent.cs` | **Catalog specialist** | A2A client for catalog availability (`check_catalog_availability`). |
| `scenarios/17-A2AStoreOperationsNetwork/src/Products/Services/Agents/PromotionsAgent.cs` | **Promotions specialist** | A2A client for promotions/offers. |
| `scenarios/17-A2AStoreOperationsNetwork/src/Products/Services/Agents/BusinessInsightsAgent.cs` | **Insights specialist** | A2A client for business insights/reviews (`get_insights`). |
| `scenarios/17-A2AStoreOperationsNetwork/src/eShopAppHost/Program.cs` | **The network topology** | Registers the three agents + orchestrator; the resource labels are the discovery names. |
| `scenarios/17-A2AStoreOperationsNetwork/src/Store/Components/Pages/Search.razor` | **The A2A UI** | The Search Type dropdown + the enriched product cards. |

## Talking points / FAQ

- **"Is the network live?"** Yes. Three separate agent services run in the Dashboard; the orchestrator
  calls them per product and merges the result onto the card.
- **"What did you rename?"** The agent **roles/classes** and their service-discovery labels were
  remapped to store-ops (Catalog, Promotions, Business Insights). Behavior is unchanged; it builds clean.
- **"MCP vs A2A?"** MCP (Demo 4) = an agent calling **tools**. A2A (this demo) = **agents calling
  agents**. Same store, two collaboration models.
- **"Could a specialist be a hosted or local agent?"** Yes — each agent is an independent service;
  it could be hosted in Azure or run locally. That's the extensibility point.

## Notes

- Always start from `scenarios/17-A2AStoreOperationsNetwork/src/eShopAppHost` so `aspire start`
  attaches to **this** scenario.
- Resource labels (`catalog-agent`, `businessinsights-agent`) and the matching `BaseAddress`
  service-discovery names were changed **together** — keep them in sync if you edit further.
- This is **Demo 5** in the session. Previous: Demo 4 (MCP Store Tools). Next: Demo 6 / bonus
  (MAF Dev UI, Scenario 18).
