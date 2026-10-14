# Scenario 18: from the Blazor AI blog to the code

[Quick start](../README.md) | [Detailed notes](DETAILS.md) | [Requirements](REQUIREMENTS_AND_TROUBLESHOOTING.md) | [Screenshots](WALKTHROUGH.md)

## What the blog explains

[Build Agentic UI with the new Blazor AI components](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/)
introduces experimental Blazor components that turn streamed agent interactions into
application-owned UI. It covers chat, typed tool-result blocks, frontend tools,
human-in-the-loop backend approvals, and shared/predictive state. Its linked
[AgenticUI sample](https://github.com/danroth27/AgenticUI) demonstrates those patterns
on separate pages.

Scenario 18 applies a focused subset to eShopLite's existing outdoor catalog. The
model can find a product and propose a cart action; ordinary Blazor components own
the cards, buttons, authoritative catalog price, and session cart.

| Blog concept | What we use here | Read the implementation |
|---|---|---|
| `ChatPage` | Chat shell, message list, and text input | [Home.razor](../src/Store/Components/Pages/Home.razor) |
| `UIAgent` wrapping `IChatClient` | Converts remote response updates into observable blocks for one conversation | [ShoppingConversation.cs](../src/Store/Components/ShoppingConversation.cs) |
| Remote agents over AG-UI | Store uses `AGUIChatClient`; Agent uses `AddAGUIServer` and `MapAGUIServer("/agent", ...)` | [Home.razor](../src/Store/Components/Pages/Home.razor), [Agent/Program.cs](../src/Agent/Program.cs) |
| `[ToolBlock]`, `[ToolParameter]`, `[ToolResult]` | Source-generated types for `search_products` and `get_product` | [ProductSearchBlock.cs](../src/Store/Components/ProductSearchBlock.cs) |
| `BlockRenderer<TBlock>` | Renders pending catalog operations, then typed product cards | [Home.razor](../src/Store/Components/Pages/Home.razor), [ProductCards.razor](../src/Store/Components/ProductCards.razor) |
| `RegisterUIAction` / `UIActionBlock` | Pauses `confirm_add_to_cart` until the user supplies a decision | [Home.razor](../src/Store/Components/Pages/Home.razor), [CartApproval.razor](../src/Store/Components/CartApproval.razor) |
| Observable conversation state | Activity, pending-input, and safe failure messages | [ConversationNotice.razor](../src/Store/Components/ConversationNotice.razor) |
| Aspire orchestration | Connects Store, Agent, Products, SQL, and server-side model settings | [eShopAppHost/Program.cs](../src/eShopAppHost/Program.cs) |

**Not implemented:** `FunctionApprovalBlock` for consequential backend tools,
`UIAgent<TState>` shared state, predictive state, rich Markdown parsing, multimodal input,
or durable conversations. They are useful blog patterns, but are not required for
this session-only cart. Do not confuse a frontend cart approval with permission to
place an order.

## Read the source in this order

### 1. Start with the resource graph

[eShopAppHost/Program.cs](../src/eShopAppHost/Program.cs) composes the separate services.
In normal mode, SQL feeds Products, Products uses embeddings, Agent calls Products,
and Store connects to both APIs through service discovery.
The four model parameters are injected only into the services that need them.
[aspire.config.json](../aspire.config.json) makes `aspire start` select this AppHost.

### 2. See the UI before reading model code

[Home.razor](../src/Store/Components/Pages/Home.razor) creates `AGUIChatClient`, declares
the cart action with `AIFunctionFactory.Create`, and calls
[ShoppingConversation.Create](../src/Store/Components/ShoppingConversation.cs).
That factory registers generated tool blocks and, when supplied, the frontend UI action.

The `ChatPage.MessageListContent` renderers choose UI based on block type, not
model-generated HTML. [ProductCards.razor](../src/Store/Components/ProductCards.razor)
renders shared catalog records. Tool arguments/results use the explicit camelCase
JSON names in [Contracts/Product.cs](../src/Contracts/Product.cs).

### 3. Follow the agent's read-only tools

[Agent/Program.cs](../src/Agent/Program.cs) builds a `ChatClientAgent` with two backend tools:
`search_products` and `get_product`. [ProductTools.cs](../src/Agent/ProductTools.cs)
validates input and calls Products over HTTP. It does not have a database connection
or a tool that mutates the cart.

The configured model uses the OpenAI-compatible **Responses API** through the
Microsoft.Extensions.AI adapter. `gpt-6-luna` tools with reasoning are not supported
by Chat Completions in the tested integration. The narrow `OPENAI001` suppression
acknowledges an experimental SDK surface; it is not a runtime error handler.

### 4. Find the source of truth

[Products/Program.cs](../src/Products/Program.cs) exposes read-only catalog endpoints,
creates/seeds SQL with stable IDs, and initializes the embedding index.
[Products/Catalog.cs](../src/Products/Catalog.cs) contains the nine products and keyword/
cosine search. [SearchInput](../src/Contracts/Product.cs) enforces query, price, and
result bounds. The in-process vector index is rebuilt on restart, not persisted as a
separate vector database.

The catalog is reused from [Scenario 01](../../01-SemanticSearch/README.md), but this is
a standalone implementation, not a second UI writing to Scenario 01's database.
It does not copy the baseline's checkout or generated Azure deployment infrastructure.

### 5. Inspect the approval boundary carefully

[CartApproval.razor](../src/Store/Components/CartApproval.razor) reloads the requested
product before enabling approval. Its buttons overwrite any model-supplied
`accepted` argument with the actual human decision, then call `UIActionBlock.InvokeAsync`.

[CartState in CatalogClient.cs](../src/Store/Services/CatalogClient.cs) returns immediately
on rejection. On approval it reloads the catalog price, updates only this circuit's
cart, and caps quantity at ten per product. Its result then lets the model continue.
An assistant sentence saying "added" is not what changes the cart.

## Trace a search and a cart action

```text
User prompt
  -> Home / ChatPage -> UIAgent -> AGUIChatClient
  -> POST /agent -> ChatClientAgent -> Foundry Responses
  -> search_products -> Products API -> catalog + embeddings
  -> AG-UI tool result -> generated ProductSearchBlock -> ProductCards

Request to add a product
  -> model requests confirm_add_to_cart
  -> UIActionBlock pauses -> CartApproval displays catalog product/price
  -> human clicks Approve or Reject -> trusted decision supplied to InvokeAsync
  -> CartState rejects unchanged, or reloads catalog and adds one item
  -> tool result returns to agent -> assistant can confirm the outcome
```

In Interactive Server Blazor, "frontend action" means UI-owned code in the Blazor
circuit; it does not mean model credentials or business authority move into browser
JavaScript.

## Read the tests next

| File | What it helps you understand |
|---|---|
| [CatalogAndCartTests.cs](../src/Scenario.Tests/CatalogAndCartTests.cs) | Bounds, semantic ranking, endpoint validation, cart isolation, rejection, and authoritative prices. |
| [ProtocolTests.cs](../src/Scenario.Tests/ProtocolTests.cs) | Actual AG-UI HTTP streaming through MAF, generated cards, and frontend actions awaiting a decision. |
| [ModelTransportTests.cs](../src/Scenario.Tests/ModelTransportTests.cs) | Real-mode registration sends streaming requests and tool definitions to Responses using the configured deployment, with a mock provider. |
| [ConversationNoticeTests.cs](../src/Scenario.Tests/ConversationNoticeTests.cs) | Transport failure becomes a visible notice without provider diagnostics. |

[DemoChatClient.cs](../src/Agent/DemoChatClient.cs) exists only for the explicit offline
fixture and protocol tests. Do not mistake its fixed tool calls for model reasoning.
For complete build commands and actual real-model validation scope, see
[detailed notes](DETAILS.md#build-and-tests).
