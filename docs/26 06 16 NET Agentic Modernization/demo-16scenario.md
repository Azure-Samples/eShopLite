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

1. **Set context (20s):**
   "We put AI inside the app three times. Now we expose the store's abilities as MCP tools — so any
   agent can call them, and we stay in control of which tools are allowed."
2. **Open the Store:** from the Aspire Dashboard open the `store` endpoint. The Products grid is
   already populated **through an MCP tool** (`LookupProductByName`) — mention that the home grid is
   itself an MCP call.
3. **Open Search:** click **Search** in the nav. Expand **Available MCP Server Tools** — point at the
   list. These are the remapped store-ops tools published by the MCP server, each with a checkbox.
4. **Select the catalog tools:** check `SearchStoreCatalog` and `LookupProductByName`.
5. **Ask a catalog question:** type `do you have something for winter camping` and click **Search**.
   The model picks a tool and answers.
6. **Show the proof — Function Call Details:** expand the **Function Call Details** card. Point at
   **Function Name** = `SearchStoreCatalog` (or `LookupProductByName`). "The page shows the exact MCP
   tool the model called — not a guess."
7. **Add a trip-context tool:** also check `GetTripWeather` (or `GetDestinationGuide`), then ask
   `what gear do I need for a rainy hike this weekend`. Show that the model can now reach a
   trip-context tool to ground its recommendation.
8. **Demonstrate control (the key move):** **uncheck** a tool and re-run. The model can no longer call
   it. "Tool exposure is opt-in. Read-only by default. This is how you keep an agent on rails."
9. **Close with value line (15s):**
   "The store is now composable. Its catalog and trip-context abilities are named, callable tools —
   ready for any MCP-aware agent, with you deciding what's on the table."

## The ask (what the Search page sends)

You type a natural-language question; the MCP client passes it to the model **with only the tools you
checked**. Say one of these out loud as you run it:

```text
do you have something for winter camping        → SearchStoreCatalog / LookupProductByName
what gear do I need for a rainy hike this weekend → + GetTripWeather / GetDestinationGuide
```

## Expected output

```text
Search Products
[ Available MCP Server Tools ]
  [x] SearchStoreCatalog   Semantic catalog search over the product database
  [x] LookupProductByName  Keyword catalog lookup by name/term
  [ ] GetTripWeather       Destination weather to guide outdoor-gear recommendations
  [ ] GetDestinationGuide  Destination/park guide for trip-gear recommendations
  [ ] ResearchProductsOnline  Online research feeding a catalog recommendation

Function Call Details
  Function Call ID:   <id>
  Function Name:      SearchStoreCatalog

Response
  <markdown answer>

Product List
  <matching products with image, name, description, price>
```

## How the demo maps to MCP concepts

| What you see | MCP concept | Where in the code |
|---|---|---|
| The tool list with checkboxes | Tools published by an MCP server | `eShopMcpSseServer/Tools/*.cs` (`[McpServerTool(Name = ...)]`) |
| Selecting which tools are allowed | Client-side tool gating | `Store/Components/Pages/Search.razor` (selected tools) |
| Function Name shown after a search | The tool the model actually invoked | `McpFunctionCallName` set in the tool methods |
| The Products grid loading | An MCP tool call on page load | `Store/Components/Pages/Products.razor` → `LookupProductByName` |

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
