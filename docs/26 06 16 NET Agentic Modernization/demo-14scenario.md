# Demo 1 Walkthrough - Scenario 14 Product Discovery Copilot

> **Format: code walkthrough — no app run.** For this session, Demo 1 is **not** a live
> demo. We do **not** launch the scenario. We explain *how the data is grounded* and read
> the code in
> `scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs`
> to show exactly how intent-based discovery and grounded explanations work.

## Objective and presenter story

**Objective:** show that a modernized .NET app can turn a shopper's natural-language
question into relevant catalog results **and** a grounded, catalog-only explanation of why
each product matches — and prove it **from the code**, not from a running UI. No generic
chatbot, no invented products.

**Story line (told over the code):**

1. The app is already modernized: Aspire wires `products`, `store`, and `sql`.
2. At startup, every catalog product is embedded into an in-memory vector store.
3. When a shopper asks a question in plain language, the app embeds the **question** and
   vector-matches it against the catalog.
4. Only the matched products (plus the question) are handed to the chat model, which writes
   a short explanation grounded **only** in those products.
5. This proves AI value comes from the app's own catalog data — the model literally cannot
   talk about products the vector search did not return.

## What makes this "Product Discovery Copilot"

- **Intent-first discovery:** the shopper expresses intent in natural language; the app
  matches on meaning (vector similarity), not keywords.
- **Grounded by construction:** the chat model only ever sees the products the vector search
  returned, plus the user's question. It is told to compare them and stay on-catalog.
- **Honest by default:** a system prompt scopes answers to outdoor-camping products and tells
  the model to say "I don't know that." when there is no match; the `Score > 0.3` similarity
  gate is what triggers the no-match / no-invented-products behavior.

## The one file to read: `MemoryContext.cs`

Everything below lives in
`scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs`.
Show it on screen and walk these two methods in order.

### 1. Startup grounding — `InitMemoryContextAsync` (lines ~34–82)

This runs once at startup and builds the grounded knowledge base.

- **In-memory vector store (lines 37–39):** an `InMemoryVectorStore` collection named
  `products` is created — no external vector DB is needed for this scenario.
- **System prompt defined (line 42):** the guardrail that keeps the model honest:

  ```text
  You are a useful assistant. You always reply with a short and funny message.
  If you do not know an answer, you say 'I don't know that.'
  You only answer questions related to outdoor camping products.
  For any other type of questions, explain to the user that you only answer
  outdoor camping products questions. Do not store memory of the chat conversation.
  ```

- **Every product becomes a vector (lines 46–77):** for each catalog product, the app builds
  a text line — `"[Name] is a product that costs [Price] and is described as [Description]"`
  (line 56) — calls `GenerateVectorAsync` (line 67), and upserts the resulting embedding into
  the collection (line 70). **This is the grounding data:** the catalog itself, embedded.

> Talking point: the knowledge the model is allowed to use is built *from the database*, at
> startup, from real catalog rows — not from the model's training data.

### 2. Query-time grounding — `Search` (lines ~84–150)

This is the path that runs for every shopper question. Walk the numbered steps:

1. **Embed the question (lines 98–99):** `GenerateVectorAsync(search)` turns the shopper's
   natural-language text into a query vector — the same embedding space as the catalog.
2. **Vector search, top 3 (line 105):** `SearchAsync(vectorSearchQuery, top: 3)` returns the
   nearest catalog products by meaning.
3. **The honesty gate (line 107):** `if (resultItem.Score > 0.3)` — only sufficiently similar
   products are kept. Off-catalog questions ("something to paint my room white") fall below
   this threshold, so nothing is added and the model has nothing to invent from.
4. **Build the grounded context (lines 109–118):** each kept product is loaded from the DB and
   appended to a `Found Products` block with **name, description, price** — the only product
   facts the model will see.
5. **Build the grounding prompt (lines 123–130):** the question and the found-products block
   are composed into a single instruction:

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

6. **Two system messages, then the call (lines 132–141):** the chat request is built from the
   **system prompt** (the on-catalog/honesty guardrail) *and* the **grounding prompt** (only
   the found products). `GetResponseAsync` returns the explanation, which becomes
   `response.Response`.
7. **Logged for proof (line 138):** the full message list is serialized to the log, so you can
   point out: "this is the exact grounded prompt the model received — nothing else."

> Key line to land the point: **the model is handed only the products the vector search found.**
> If the search finds nothing above `0.3`, the response stays the default
> "I don't know the answer for your question…" (line 93) — the app cannot fabricate products.

## Supporting files (mention, don't deep-dive)

Show these only if there's time; the grounding story lives entirely in `MemoryContext.cs`.

| File | Role | One-liner |
|---|---|---|
| `src/Products/Memory/MemoryContext.cs` | **Discovery pipeline** | Startup embedding of the catalog + query-time embed → vector match (`> 0.3`) → grounded prompt → chat. **The file to read.** |
| `src/Products/Endpoints/ProductEndpoints.cs` | Discovery endpoints | `GET /api/product/search/{term}` (literal keyword) vs `GET /api/aisearch/{term}` (semantic) — same input, two behaviors. `aisearch` calls `MemoryContext.Search`. |
| `src/Store/Components/Pages/Search.razor` | Discovery UI | The single search box + `Use Semantic Search` toggle; renders the model's Markdown answer above the product table. |
| `src/Store/Services/ProductService.cs` | Delegation | `Search(term, semanticSearch)` chooses `/api/aisearch/...` vs `/api/product/search/...`. |
| `src/eShopAppHost/Program.cs` | Orchestration | Aspire composition of `sql`, `products`, `store`; Azure OpenAI (chat + embeddings) wired into `products`. |

## Architecture in one line

Keyword search hits `GET /api/product/search/{term}` (literal name match). Semantic search
hits `GET /api/aisearch/{term}` → `MemoryContext.Search`: it embeds the question with
**text-embedding-3-small**, vector-searches the in-memory catalog store (top 3, similarity
`> 0.3`), then asks **gpt-5-mini** to explain the matches using **only** the found products.

## Suggested on-screen flow (no run)

1. **Set context (20s):** "We won't run this one. I want to show you *how* a modernized app
   keeps an AI answer honest — by grounding it in its own catalog. It's all in one file."
2. **Open `MemoryContext.cs`.** Scroll to `InitMemoryContextAsync`. Explain: at startup, every
   product is embedded into an in-memory vector store (lines 46–77). Show the system prompt
   (line 42) — the on-catalog/honesty guardrail.
3. **Scroll to `Search`.** Walk the seven steps above: embed question → top-3 vector search →
   the `> 0.3` gate (line 107) → build `Found Products` → the grounding prompt (lines 123–130)
   → two system messages → chat call.
4. **Land the grounding point:** "The model only ever sees the products the vector search
   returned. No match above `0.3` → nothing to talk about → it can't invent products."
5. **(Optional) Show the contrast:** point at `ProductEndpoints.cs` — the literal
   `product/search` route vs the `aisearch` route — to show keyword vs intent are two explicit,
   inspectable paths.
6. **Close with value line (15s):** "Same app, same catalog — grounding is just a few lines:
   embed, match, prompt with only what you found. That's how you get intent-based discovery
   without hallucinated products."

## Talking points / FAQ

- **"Where does the model's knowledge come from?"** The catalog, embedded at startup
  (`InitMemoryContextAsync`). Not training data.
- **"What stops it inventing products?"** Two things working together: the `Score > 0.3` gate
  (only real, similar catalog rows reach the prompt) and the system prompt (on-catalog +
  "I don't know that.").
- **"Is a vector database required?"** No — this scenario uses an **in-memory** store
  (CommunityToolkit.VectorData.InMemory). Other scenarios show external/SQL vector stores.
- **"Cloud or local?"** Here embeddings + chat run on **Azure OpenAI** (the cloud-grounded
  discovery story). The grounding *technique* is identical regardless of the model host.

## Notes

- This walkthrough intentionally has **no prerequisites, no secrets setup, and no run/launch
  steps** — it is a code reading. If you ever do want to run it live, the scenario is a normal
  Aspire app (`sql` + `products` + `store`) that needs the four `Parameters:AzureOpenAI*`
  secrets; that is out of scope for this session's Demo 1.
- Line numbers above match `MemoryContext.cs` as of this writing; if the file changes, re-check
  the `Score > 0.3` gate and the grounding-prompt block before presenting.
