# Demo 2 Playbook - Scenario 14 Product Discovery Copilot

## Objective and presenter story

**Objective:** show that a modernized .NET app can turn a shopper's natural-language
question into relevant catalog results **and** a grounded, catalog-only explanation of why
each product matches — no generic chatbot, no invented products.

**Story line:**

1. The app is already modernized: Aspire wires `products`, `store`, and `sql`.
2. A shopper types a question in plain language ("do you have something for a rainy day").
3. Keyword search treats it as a literal string and finds little or nothing.
4. Semantic search embeds the question, finds the closest catalog products, and the model
   writes a short, friendly explanation grounded **only** in the products that were found.
5. This proves AI value comes from the app's own catalog data, not from a chatbot that
   hallucinates products that don't exist.

## What makes this "Product Discovery Copilot"

- **Intent-first discovery:** the shopper expresses intent in natural language; the app
  matches on meaning (vector similarity), not keywords.
- **Grounded explanations:** the chat model is given the found products and asked to
  compare them to the question. The system prompt keeps it on outdoor-camping products and
  tells it to say "I don't know that." when there is no match — so it stays honest.
- **One toggle, two behaviors:** the same search box runs either a literal keyword search or
  the semantic + grounded-explanation flow, so the contrast is visible live.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running (SQL Server 2025 runs in a container).
- Scenario source at:
  - `D:\azure-samples\eShopLite\scenarios\14-ProductDiscoveryCopilot`
- **Azure OpenAI** deployments reachable from this scenario:
  - a chat deployment (demo default `gpt-5-mini`)
  - an embeddings deployment (demo default `text-embedding-3-small`)
- Set the AppHost parameter secrets once (run from the scenario `src` so `--apphost` resolves):

  ```pwsh
  cd D:\azure-samples\eShopLite\scenarios\14-ProductDiscoveryCopilot\src
  aspire secret set Parameters:AzureOpenAIEndpoint "https://<your-resource>.openai.azure.com/" --apphost eShopAppHost/eShopAppHost.csproj
  aspire secret set Parameters:AzureOpenAIApiKey "<your-key>" --apphost eShopAppHost/eShopAppHost.csproj
  aspire secret set Parameters:AzureOpenAIDeploymentName "gpt-5-mini" --apphost eShopAppHost/eShopAppHost.csproj
  aspire secret set Parameters:AzureOpenAIEmbeddingsDeploymentName "text-embedding-3-small" --apphost eShopAppHost/eShopAppHost.csproj
  ```

  > The repo also ships `scripts/Set-AzureOpenAISecrets.ps1` to set these four values for
  > every scenario at once. The model names are demo defaults — point the deployment-name
  > parameters at whatever chat/embeddings deployments your Azure OpenAI resource exposes.

Quick validation:

```pwsh
dotnet --version
aspire --version
docker --version
```

## Run instructions (preferred: `aspire start`)

### Preferred flow

```pwsh
cd D:\azure-samples\eShopLite\scenarios\14-ProductDiscoveryCopilot\src\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:

- `sql`, `products`, and `store` resources are `Running`.
- `products` logs show "Done fill products in vector db" (the in-memory vector store is
  populated — embeddings were generated for every catalog product at startup).
- Open the Dashboard URL shown by Aspire and pin it in a browser tab.

### Alternative A (if Aspire CLI behaves unexpectedly)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\14-ProductDiscoveryCopilot\src\eShopAppHost
dotnet run
```

### Alternative B (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\14-ProductDiscoveryCopilot\src
dotnet restore
dotnet build
cd .\eShopAppHost
aspire start --non-interactive
```

> **Tip — launch the right scenario.** `aspire start` resolves the AppHost from the current
> directory. Always `cd` into **this** scenario's `eShopAppHost` (or `src`) folder before
> running it, so you don't accidentally launch a different scenario's AppHost.

## Architecture in one line

Store **Search** page -> `products` service. Keyword search hits
`GET /api/product/search/{term}` (literal name match). Semantic search hits
`GET /api/aisearch/{term}`, which embeds the question with **text-embedding-3-small**,
vector-searches the in-memory product store (top 3, similarity > 0.3), then asks
**gpt-5-mini** to write a grounded explanation using only the found products.

## Key files to talk about

Show these three files to explain how discovery + grounded explanation works end to end.

| File | Role | What to say |
|---|---|---|
| `src/Store/Components/Pages/Search.razor` | **Discovery UI** | The single search box plus the `Use Semantic Search` toggle. On submit it calls `ProductService.Search(term, smartSearch)`; the model's Markdown answer renders above the product table. This is where keyword vs. semantic is chosen live. |
| `src/Products/Endpoints/ProductEndpoints.cs` (+ `ProductAiActions.cs`) | **Discovery endpoints** | `GET /api/product/search/{search}` is the literal keyword route; `GET /api/aisearch/{search}` is the AI route. Same input, two very different behaviors — this is the contract the Store calls. |
| `src/Products/Memory/MemoryContext.cs` | **Discovery pipeline** | The heart of the demo: `InitMemoryContextAsync` embeds every catalog product into an in-memory vector store at startup; `Search` embeds the shopper's question, pulls the top matches (score > 0.3), builds a grounded prompt with the found products, and calls the chat model. The system prompt scopes answers to outdoor-camping products and allows "I don't know that." so it never invents items. |

Presenter note: the discovery flow is grounded by construction — the model only ever sees
the products the vector search returned, plus the user's question. It is told to compare them
and stay on-catalog.

## Step-by-step live demo script

1. **Set context (20s):**
   "This is the modernized eShopLite baseline. Now I'll let a shopper search the way they
   actually talk — and have the app explain its results."
2. **Open Store UI:** from Aspire Dashboard, click the `store` endpoint, then open **Search**.
3. **Keyword baseline (semantic OFF):** leave `Use Semantic Search` unchecked and run a
   natural-language query, e.g. `do you have something for a rainy day`. Point out that the
   literal name search returns little/nothing and no explanation.
4. **Turn on discovery:** check `Use Semantic Search` and run the **same** query. Now the
   app returns relevant products **and** a short, friendly grounded explanation above the
   table. Call out: "Same question, but now it matched on meaning and explained why."
5. **Run 2-3 intent queries** (see "Exact prompts to run") to show range — cooking, cold-
   weather hiking, a budget/trip framing.
6. **Show honesty / grounding:** run an off-catalog or no-match query (e.g.
   `something to paint my room white`). The assistant stays on-catalog and does not invent
   products — that is the system prompt + the > 0.3 similarity gate working.
7. **(Optional) Show the route:** in Aspire Dashboard open `products` logs while you search;
   the chat history is logged, so you can show the exact grounded prompt the model received.
8. **Callout architecture in one line:** Store sends the term to `products`; semantic search
   embeds it, vector-matches the catalog, and the model explains the matches it was given.
9. **Close with value line (15s):**
   "Same app, same catalog — but now customers find products by intent, and every
   explanation is grounded in real catalog data."

## Exact prompts to run

These are typed into the **Search** box. Run each one **twice** to contrast: first with
`Use Semantic Search` **off** (keyword), then **on** (semantic + grounded explanation).

**Queries that return strong matches (use these for the main beats):**

```text
do you have something for cooking
do you have something for a rainy day
I need gear for hiking in cold weather
what should I take on a camping trip
```

**Intent / framing queries (show range):**

```text
show me something useful for a summer trip
I want to keep warm at night outdoors
```

**No-match / off-catalog query (show honesty — no invented products):**

```text
something to paint my room white
```

There is no separate prompt box — the search term **is** the prompt. With semantic search
on, the term is embedded and the model is handed only the products the vector search found,
together with this instruction (built in `MemoryContext.Search`):

```text
You are an intelligent assistant helping clients with their search about outdoor products.
Generate a catchy and friendly message using the information below.
Respond using Markdown with concise sections and bullet lists when helpful.
Add a comparison between the products found and the search criteria.
Include products details.
    - User Question: {search}
    - Found Products:
{the products returned by the vector search, with name / description / price}
```

## Expected output

With semantic search **on** and Azure OpenAI reachable, the Search page shows two things:

1. A **Markdown answer** above the table — a short, friendly message that compares the found
   products to the question and lists them with details (name, why it matches, price).
2. The **product table** populated with the matching catalog items (the same products the
   model was grounded on).

Example shape (wording varies per model/run):

```text
Looking for something for a rainy day? Here's a great match from our catalog:

- **Outdoor Rain Jacket** ($49.99) — "keeps you warm and dry in all weathers", so it's
  built exactly for wet conditions and packs down for sudden showers.

Compared to your request, this is the catalog item most focused on staying dry outdoors.
```

Keyword terms like `tent`, `jacket`, `cookware`, or `lantern` line up with real seeded
products (e.g. **Camping Tent**, **Outdoor Rain Jacket**, **Camping Cookware**,
**Camping Lantern**), which is handy for the keyword-vs-semantic contrast.

With semantic search **off**, the same query runs a literal name match: typically few or no
rows and **no** explanation. That contrast is the point of the demo.

## Fallback plan (Azure OpenAI unavailable)

If the embeddings/chat deployment is unreachable, the semantic path returns an error message
in the response area (e.g. `An error occurred: ...`) instead of a grounded answer.

1. **Stay on the keyword path:** turn `Use Semantic Search` **off**. Keyword search hits
   `GET /api/product/search/{term}` and needs no model — run literal terms like `tent`,
   `jacket`, `backpack` to keep results flowing.
2. Say: "The discovery layer needs the embeddings + chat model; the rest of the app — catalog,
   search, cart — keeps working."
3. To recover the AI path: re-check the four `Parameters:AzureOpenAI*` secrets (endpoint,
   key, deployment names), confirm the deployments exist in your Azure OpenAI resource, then
   restart the AppHost and retry semantic search.
4. Continue to Demo 3 without retry loops.

## Code files to show and story mapping

| Story beat | File | What to show |
|---|---|---|
| "Aspire wires the scenario." | `scenarios/14-ProductDiscoveryCopilot/src/eShopAppHost/Program.cs` | `sql`, `products`, `store` composition; Azure OpenAI parameters (endpoint/key/chat/embeddings) wired into `products`; publish-mode provisions AOAI + App Insights via azd. |
| "Products is AI-capable but catalog-grounded." | `scenarios/14-ProductDiscoveryCopilot/src/Products/Program.cs` | `AddServiceDefaults`, AOAI client setup, `AddChatClient`, `AddEmbeddingGenerator`, and the startup `InitMemoryContextAsync` that fills the vector store. |
| "Two explicit, inspectable search routes." | `scenarios/14-ProductDiscoveryCopilot/src/Products/Endpoints/ProductEndpoints.cs` | `/api/product/search/{search}` (keyword) and `/api/aisearch/{search}` (semantic) side by side. |
| "Discovery + grounded explanation is local to one method." | `scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs` | Embed question -> vector search top 3 (> 0.3) -> build grounded prompt from found products -> chat completion; system prompt keeps it on-catalog and honest. |
| "UI toggles keyword vs. semantic live." | `scenarios/14-ProductDiscoveryCopilot/src/Store/Components/Pages/Search.razor` | The `Use Semantic Search` switch, `DoSearch`, and Markdown rendering of the grounded answer. |
| "Store just delegates to the right route." | `scenarios/14-ProductDiscoveryCopilot/src/Store/Services/ProductService.cs` | `Search(term, semanticSearch)` picks `/api/aisearch/...` vs `/api/product/search/...`. |

## Notes

- The vector store is **in-memory** (CommunityToolkit.VectorData.InMemory): embeddings are
  generated for every catalog product at startup, so the first semantic search after launch
  is instant. No external vector database is required for this scenario.
- Embeddings + chat both run on **Azure OpenAI** in this scenario (this is the cloud-grounded
  discovery story). The fully-local variant of discovery is covered by other scenarios; here
  the point is intent-first search + grounded explanation over the app's own catalog.
- The similarity gate (`Score > 0.3`) is what produces the "I don't know that." / no-invented-
  products behavior on off-catalog queries — worth calling out as the honesty guardrail.
