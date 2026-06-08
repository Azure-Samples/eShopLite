# Demo 4 Playbook - Scenario 16 MCP Store Operations Tools

> **Format: fully live.** This scenario exposes the store's capabilities as **Model Context
> Protocol (MCP) tools**. The Store's Search page lists the tools published by the MCP server,
> lets you pick which ones the model may call, and shows **which tool the model actually invoked**
> (the `McpFunctionCallName`) for every answer. The toolset has been **remapped to a store-operations
> vocabulary** so the names tell the story on screen.

## Objective and presenter story

**Objective:** show that the same modernized store can expose its abilities as **discrete, named,
read-only MCP tools** that any MCP-aware model or agent can call — and that you stay in control of
*which* tools are available for a given request.

**Story line:**

1. Demos 1-3 put AI *inside* the app. MCP turns the app's abilities **into tools** other agents can use.
2. The Store's Search page is an MCP client: it lists the tools the MCP server publishes.
3. You **select which tools** the model is allowed to call (read-only by default).
4. You ask a question; the model picks a tool and answers — and the page shows the **exact tool name** it called.
5. This proves the store is now **composable**: its catalog and trip-context tools are callable building blocks, not a black box.

## Where this sits in the arc

- **Demo 1 (Product Discovery):** better answers for *users*.
- **Demo 2 (Observability Assistant, local AI):** better understanding for *developers/ops*.
- **Demo 3 (Store Intelligence):** better summaries for *business users*.
- **Demo 4 (MCP Store Tools, this demo):** the store's abilities become **callable tools** for any agent.

## The remapped store-operations toolset

The MCP server publishes these tools (names/descriptions remapped to the store-ops story; the
underlying implementations are unchanged and runnable):

| Tool name (`McpFunctionCallName`) | What it does | Backed by |
|---|---|---|
| `SearchStoreCatalog` | Semantic catalog search over the product database | Products API semantic search |
| `LookupProductByName` | Keyword catalog lookup by name/term | Products API keyword search |
| `GetTripWeather` | Destination weather to guide outdoor-gear recommendations | Weather service |
| `GetDestinationGuide` | Destination/park guide for trip-gear recommendations | Park information service |
| `ResearchProductsOnline` | Online research feeding a catalog recommendation | Online research service |

> The `SearchStoreCatalog` and `LookupProductByName` tools are the catalog core of the demo.
> The trip-context tools (`GetTripWeather`, `GetDestinationGuide`, `ResearchProductsOnline`) show
> how multiple tools can be combined to ground a recommendation.

## Current state

- Scenario 16 is derived from **Scenario 06 (Model Context Protocol)** and is **runnable**.
- Resources: `sql` + `products` + `eShopMcpSseServer` (MCP SSE server) + the trip-context backend
  services + `store` (MCP client UI).
- The Store Products page is itself an MCP client: it calls `LookupProductByName` to load the grid.
- Verified: `eShopLite-Aspire-mcp.slnx` builds clean; the Search page lists the remapped tools and
  reports the invoked tool name per answer.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source at: `D:\azure-samples\eShopLite\scenarios\16-MCPStoreOperationsTools`
- The Products service needs the four Azure OpenAI parameters (same as Scenario 01/14/15):
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
cd D:\azure-samples\eShopLite\scenarios\16-MCPStoreOperationsTools\src\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:

- `sql`, `products`, `eShopMcpSseServer`, and `store` resources are `Running`.
- Open the Dashboard URL shown by Aspire and pin the `store` endpoint in a browser tab.

### Alternative (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\16-MCPStoreOperationsTools\src
dotnet restore
dotnet build
cd .\eShopAppHost
aspire start --non-interactive
```

> Note: always launch from **this scenario's** `eShopAppHost`. Running `aspire start` from the
> wrong folder can attach to a different scenario's AppHost (a known footgun across scenarios).

## Step-by-step live demo script

### The ADVANTAGES you are demonstrating

This demo makes three concrete points about MCP tool exposure:

1. **Composability** — the store's catalog search and lookup abilities become *named, callable tools*
   that any MCP-aware model or agent can invoke. You're not re-writing the business logic; you're
   exposing what already works through a standard protocol.
2. **Opt-in / read-only control** — the client (the Search page) decides which tools the model is
   *allowed* to call for each request. Checking or unchecking a box is enough. You control the
   blast radius.
3. **Any MCP-aware agent can call them** — the tool contract is protocol-level, not app-level. The
   same `SearchStoreCatalog` and `LookupProductByName` tools that power this UI are callable by
   any agent that speaks MCP.

---

### Walkthrough

1. **Set context (20s):**
   > "Demos 1-3 put AI inside the app. MCP flips the model: now the store's abilities become
   > **tools** that any agent can call — and we stay in control of which ones are on the table."

2. **Open the Store home page** (from the Aspire Dashboard → `store` endpoint).
   Point at the Products grid:
   > "This grid is **already an MCP call** — the home page calls `LookupProductByName` on load.
   > The store was always good at finding products; MCP makes that ability *composable*."

3. **Navigate to Search** → expand **Available MCP Server Tools**.
   Walk down the checkbox list:
   > "These are the tools the MCP server publishes. Each one has a name and a description —
   > that description is literally what the model reads to decide which tool to call.
   > Right now nothing is checked: the model has no tools and will refuse to answer.
   > I control what it can do."

4. **Check only `SearchStoreCatalog` and `LookupProductByName`.**
   Type: `do you have something for winter camping` → click **Search**.
   A product grid appears.

5. **Expand Function Call Details.**
   Point at **Function Name** = `SearchStoreCatalog`:
   > "Not a guess. The page shows the exact tool name the model called — `SearchStoreCatalog`.
   > That name comes straight from the `[McpServerTool(Name = ...)]` attribute in the source code."

6. **Now check ALL tools** (use the select-all checkbox).
   Type: `what gear do I need for a rainy hike this weekend` → click **Search**.
   Show the product grid that appears alongside the markdown answer:
   > "With all tools available, the model called the catalog tool AND a trip-context tool.
   > The products are accumulated from both calls — the grid isn't empty even though a
   > weather tool also fired. The system prompt ensures the catalog is always consulted."

7. **Demonstrate control (the key move):**
   **Uncheck `SearchStoreCatalog`** (leave the trip-context tools checked) → re-run the same query.
   The product grid shrinks or disappears:
   > "I just removed one tool. The model can no longer call it. Tool exposure is **opt-in**.
   > Read-only by default. This is how you keep an agent on rails — no code change needed,
   > just don't put the tool on the table."

8. **Re-check everything** and close with the value line (15s):
   > "The store is now composable. Its catalog search, keyword lookup, weather context, and
   > destination guide are **named, callable tools** — ready for any MCP-aware agent, with you
   > deciding what's on the table per request."

## The ask (what the Search page sends)

You type a natural-language question; the MCP client passes it to the model **with only the tools you
checked** plus a system prompt that instructs the model to always call a catalog tool for product
queries. This system prompt is what ensures the product grid is reliably populated — the model is
explicitly told it must call `SearchStoreCatalog` or `LookupProductByName` for any gear question,
and may additionally use trip-context tools to enrich the answer.

Recommended demo queries (say these out loud as you run them):

```text
do you have something for winter camping
  → model calls: SearchStoreCatalog → product grid appears

what gear do I need for a rainy hike this weekend  (all tools selected)
  → model calls: SearchStoreCatalog + GetTripWeather (or GetDestinationGuide)
  → products from the catalog call are shown in the grid
  → trip context enriches the markdown answer
```

## Expected output

```text
Search Products
[ Available MCP Server Tools ]
  [x] SearchStoreCatalog      Semantic catalog search over outdoor products catalog
  [x] LookupProductByName     Keyword catalog lookup by product name
  [x] GetTripWeather          Destination weather to guide outdoor-gear recommendations
  [x] GetDestinationGuide     Destination/park guide for trip-gear recommendations
  [x] ResearchProductsOnline  Online research feeding a catalog recommendation

Function Call Details
  Function Call ID:   <id>
  Function Name:      SearchStoreCatalog

Response
  <markdown answer — the model's synthesized reply using all available tool results>

Product List
  <product grid: image, name, description, price — populated even when trip-context tools also fired>
```

## How the demo maps to MCP concepts

| What you see on screen | MCP concept | Where in the code |
|---|---|---|
| The tool list with checkboxes | Tools published by an MCP server — the `[Description]` text is what the model reads to pick a tool | `eShopMcpSseServer/Tools/*.cs` → `[McpServerTool(Name = ...)]` + `[Description(...)]` |
| Checking / unchecking tools | **Client-side opt-in gating** — `ChatOptions.Tools` only contains what the user selected | `Store/Services/McpServerService.cs` → `chatOptions.Tools = [.. selectedTools]` |
| Function Name shown after a search | The exact tool the model invoked (`McpFunctionCallName`) | Set inside each tool method; surfaced via `SearchResponse.McpFunctionCallName` |
| Product grid populated for broad queries | System prompt steers the model to always call a catalog tool | `McpServerService.CatalogSystemPrompt` constant (prepended to every turn) |
| Grid intact even when weather tool also fired | Products accumulated across all tool calls (no last-wins overwrite) | `accumulatedProducts.AddRange(...)` + dedup in `McpServerService.GetResponseAsync` |
| The Products grid loading on the home page | An MCP call on page load — the app already uses its own tools | `Store/Components/Pages/Products.razor` → `LookupProductByName` |

## Fallback plan

- If **semantic** search is unavailable (no secrets): use only `LookupProductByName` (keyword) — the
  tool-selection and Function Call Details story is identical.
- If a **trip-context** tool errors: stick to the two catalog tools; the core MCP story (publish →
  select → invoke → prove) still lands.
- If the **app** will not start: present the "Expected output" block and walk the tool list +
  Function Call Details from the screenshots in `screenshots/`, then continue to **Demo 5 (A2A)**.

## Key files to talk about

| File | Role | What to say |
|---|---|---|
| `scenarios/16-MCPStoreOperationsTools/src/eShopMcpSseServer/Tools/Products.cs` | **Catalog tools** | `SearchStoreCatalog` + `LookupProductByName` — the catalog core, with `McpFunctionCallName` so the client can show which ran. |
| `scenarios/16-MCPStoreOperationsTools/src/eShopMcpSseServer/Tools/WeatherTool.cs` | **Trip weather tool** | `GetTripWeather` — destination weather to ground gear recommendations. |
| `scenarios/16-MCPStoreOperationsTools/src/eShopMcpSseServer/Tools/ParkInformationTool.cs` | **Destination guide tool** | `GetDestinationGuide` — park/destination context for trip gear. |
| `scenarios/16-MCPStoreOperationsTools/src/eShopMcpSseServer/Tools/OnlineResearch.cs` | **Online research tool** | `ResearchProductsOnline` — research feeding a catalog recommendation. |
| `scenarios/16-MCPStoreOperationsTools/src/Store/Components/Pages/Search.razor` | **The MCP client UI** | Lists tools, gates which are callable, shows the invoked tool name. The heart of the demo. |
| `scenarios/16-MCPStoreOperationsTools/src/Store/Components/Pages/Products.razor` | **MCP on page load** | Calls `LookupProductByName` to populate the grid — an MCP call you can point at immediately. |

## Talking points / FAQ

- **"Are these tools live?"** Yes. The Store is a real MCP client; the tool list and the Function
  Name shown after each search come from the running MCP server.
- **"Did you rename real behavior?"** Only the tool **names/descriptions/prompts** were remapped to a
  store-ops vocabulary. The implementations are unchanged and runnable.
- **"Why expose tools at all?"** Composability and control: any MCP-aware agent can call these tools,
  and the client decides which tools are on the table (read-only by default).
- **"How is this different from Demo 5 (A2A)?"** MCP is **tools** an agent can call. A2A is
  **agents** talking to other agents. Demo 5 shows the multi-agent side.

## Notes

- Always start from `scenarios/16-MCPStoreOperationsTools/src/eShopAppHost` so `aspire start`
  attaches to **this** scenario.
- The remapped names are what appear on screen (tool list + Function Name), so the demo reads as a
  store-operations toolset end-to-end.
- This is **Demo 4** in the session. Previous: Demo 3 (Store Intelligence). Next: Demo 5
  (A2A Store Operations Network, Scenario 17).

## Reference links

- [Model Context Protocol (MCP) Overview](https://github.com/modelcontextprotocol) — Open standard for tools/resources that LLMs can call; `SearchStoreCatalog` and `LookupProductByName` are MCP tools published by the server.
- [MCP Specification](https://spec.modelcontextprotocol.io/specification/latest) — Complete reference for tool definitions, prompts, and client-server communication.
- [Building MCP Servers with .NET](https://github.com/modelcontextprotocol) — Examples and SDKs (including community C# support) for exposing .NET capabilities as MCP tools.
