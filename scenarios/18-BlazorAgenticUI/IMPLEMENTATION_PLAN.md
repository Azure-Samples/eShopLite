# Scenario 18: Blazor Agentic UI implementation plan

**Status:** Initial implementation available and validated locally with real Foundry models and SQL Server.

**Working name:** Blazor Agentic UI

**Baseline:** `scenarios/01-SemanticSearch`

**Purpose:** Demonstrate Blazor's experimental AI components in a familiar eShopLite product-discovery workflow without changing the existing scenarios.

See [README.md](README.md) for the three-step quick start and
[detailed notes](docs/DETAILS.md) for the implemented scope, exact package versions,
run commands, and deliberate first-version decisions. The sections below preserve the original proposal,
including work deferred beyond this initial implementation.

## Executive summary

Add an isolated scenario that turns the existing product search experience into a streaming, tool-using shopping assistant. Reuse the products API, catalog, search, and Aspire service boundaries from `01-SemanticSearch`; add a small agent service and a Blazor conversation UI built with `Microsoft.AspNetCore.Components.AI`.

The feature is a research sample, not a production-ready framework commitment. The Blazor AI components are experimental and available as a prerelease package for .NET 11. As of October 14, 2026, .NET 10 is the latest stable .NET release; .NET 11 is at RC. Keep the existing scenarios on the stable .NET 10 target and isolate the experimental scenario on .NET 11 RC until the components and framework reach GA.

## Baseline analysis

`01-SemanticSearch` is the best starting point because it already has the smallest end-to-end shopping flow needed for this demo:

- The Aspire AppHost composes the Store, Products API, and SQL Server, and wires Azure OpenAI settings.
- The Products API provides product listing, keyword search, and semantic search.
- The Store is a Blazor Web App using Interactive Server rendering. Its search page invokes `IProductService`, displays grounded catalog results, and optionally uses semantic search.
- `Products.Memory.MemoryContext` creates product embeddings, queries the in-memory vector collection, and asks `IChatClient` to explain the matching products.
- The cart and checkout already exist, but they are consequential actions and should not be handed to an autonomous model as unrestricted operations.

The scenario should preserve those existing APIs and UI flows. The assistant should call the product and search capabilities through a bounded server-side tool layer, rather than giving the model direct database access or moving model credentials into the browser.

## Blazor AI component research

The referenced [.NET Blog article](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/) introduces the experimental `Microsoft.AspNetCore.Components.AI` package. Its component model includes:

- `UIAgent` to adapt an `IChatClient` stream into observable conversation content.
- `ChatPage`, which composes `AgentBoundary`, `MessageList`, and `MessageInput`.
- `BlockRenderer<TBlock>` and content blocks for customized rendering of text, activities, and tool calls.
- `FunctionApprovalBlock` for pausing a conversation until the user approves or rejects a requested action.
- `UIAgent<TState>` and `AgentState<TState>` for typed shared state, with predictive state available for proposed changes that can be accepted or rejected.
- UI actions for functions that must execute in the UI, and generated typed tool blocks for strongly typed server tool rendering.

Basic chat and content blocks can use any `IChatClient`. A remote agent that exchanges frontend tool declarations, backend tool events, approval interrupts, shared state, and conversation identifiers with the Blazor app should use the Agent User Interaction Protocol (AG-UI), for example with `AGUI.Client` and `AGUIChatClient`. That makes a separate agent endpoint the recommended architecture for this scenario.

## Proposed architecture

```text
Blazor Store (ChatPage, UIAgent, custom block renderers)
        │ AG-UI stream (AGUI.Client)
        ▼
Agent API (Microsoft Agent Framework + AG-UI endpoint)
        │ bounded tools via HTTP
        ▼
Products API (catalog, keyword search, semantic search)
        │
        ├── SQL Server
        └── embeddings / chat model via server-side credentials

Aspire AppHost: Store + Agent API + Products API + SQL Server + model configuration
```

The Store renders the conversation and approval prompts. The Agent API hosts model orchestration and backend tools. The Products API remains the source of truth for catalog and search data. Keep the UI, agent, and product API as separately observable Aspire resources. Use the current repository model-configuration and secret-injection patterns, with credentials confined to server-side services.

## User experience and safety boundaries

1. A shopper asks for recommendations in natural language, such as “I need a lightweight tent for two people under $300.”
2. The assistant streams its response and visible progress while searching the catalog.
3. Typed product-result blocks show grounded catalog information, including price and links/actions to view the product.
4. The assistant can refine results or compare products using read-only catalog/search tools.
5. A request to add an item to the cart is shown as an approval block; the user must explicitly approve or reject it before the operation runs.
6. Checkout, payment, account changes, and order placement are not agent tools in this sample. Keep those established flows under direct user control.
7. Keep conversation state scoped to the current user/session. If persistence is included, store it server-side with explicit identity scoping and a clear retention policy; do not log prompt or conversation contents.

## Implementation phases

### Current implementation status

| Area | Status |
|---|---|
| Standalone scenario, SQL orchestration, model parameter wiring | Implemented |
| AG-UI server, bounded tools, typed cards, human-approved session cart | Implemented |
| Explicit offline demo, browser search and approval/rejection walkthrough | Verified |
| Scenario build and protocol/unit tests | Passing, 29 tests |
| Shared provisioning scripts and automatic AppHost discovery | Offline regression checks passing |
| Real Foundry model and SQL-backed end-to-end run | Verified with gpt-6-luna via Responses and text-embedding-3-small; search, cards, cart rejection/approval, missing-specification response, and restart |
| Repository-wide package/.NET servicing refresh | 17 existing scenarios refreshed and build/test verified; 14-MAFDevUI blocked by unavailable required Azure tool |
| Production identity/deployment and shared/predictive state | Deferred, not part of the initial implementation |

### 1. Establish supported versions and package inventory

- As of October 14, 2026, use .NET SDK `10.0.402` (latest stable .NET 10 servicing runtime `10.0.13`) and Aspire `13.6.1` as the stable upgrade targets; recheck official release pages when implementation begins.
- Keep existing scenarios on the latest stable .NET 10 servicing release and target framework `net10.0`; upgrading an SDK patch does not require changing the target framework moniker.
- Update the repository's existing Aspire packages together to one compatible stable release, then update each scenario's remaining NuGet references to the latest compatible stable versions. Do not bulk-replace versions without checking package compatibility and release notes.
- For this isolated scenario only, use the .NET 11 RC SDK (`11.0.0-rc.2` as of October 14, 2026) and prerelease `Microsoft.AspNetCore.Components.AI` package needed by the article. Clearly label it experimental and do not promote it to stable scenarios before GA.
- Record exact resolved package versions in project files and validate restore/build/test for every affected scenario. Preserve intentional prerelease dependencies only where required.

### 2. Create the scenario skeleton

- Create `scenarios/18-BlazorAgenticUI` from `01-SemanticSearch`, preserving a standalone solution, Store, Products API, entities, service defaults, and Aspire AppHost.
- Update scenario naming, README, solution/project references, local run instructions, configuration documentation, and the root scenario table.
- Keep changes isolated from `01-SemanticSearch` and other scenario copies.

### 3. Add the agent endpoint and safe tools

- Add an Agent API project using Microsoft Agent Framework and its AG-UI integration.
- Register the model client using the established server-side configuration and credential patterns.
- Implement narrow read-only tools for catalog listing, keyword search, semantic search, and product details. Validate and bound user-controlled input and result counts.
- Implement add-to-cart as a separate approval-required action only if the existing cart API supports it cleanly; do not add checkout or payment tools.
- Return product data from trusted APIs; treat model-generated prose as untrusted and do not use it as authoritative prices, inventory, or purchase instructions.

### 4. Build the Blazor agentic UI

- Add `Microsoft.AspNetCore.Components.AI` as a prerelease dependency and `AGUI.Client` for the remote agent connection.
- Register an AG-UI-backed `IChatClient` and create a `UIAgent` for each correctly scoped conversation.
- Build the conversation page with `ChatPage`, `MessageList`, and `MessageInput`.
- Add custom blocks for streamed search activity, grounded product cards, and approval prompts using `BlockRenderer<TBlock>`.
- Require explicit user approval before invoking cart mutation; render clear pending, approved, rejected, failed, and completed states.
- Add typed shared state only for a concrete UI need, such as the current product shortlist. Map only validated agent state events; do not treat shared/predictive state as trusted business data.

### 5. Wire Aspire, observability, and configuration

- Add the Agent API to the AppHost and wire references, health checks, and wait relationships consistently with existing scenarios.
- Inject model endpoint, deployment, and credentials using Aspire parameters/secrets locally and managed identity or the repository's established Azure deployment approach in publish mode.
- Use `scripts\Initialize-FoundryModels.ps1` for shared local model provisioning and secret configuration, with `gpt-6-luna` as the default chat model and `text-embedding-3-small` for embeddings. Validate availability in the selected region; do not substitute a different chat model silently.
- Give the new AppHost a unique `UserSecretsId`. Both setup scripts now discover its `eShopAppHost.csproj` automatically.
- Ensure conversation content, prompts, credentials, and tool arguments are not written to logs or traces. Log operational metadata such as durations, tool names, and outcome categories.
- Keep the scenario runnable without changing existing AppHosts.

### 6. Validate and document

- Unit-test tool input bounds, product grounding, approval/rejection behavior, and state mapping.
- Integration-test AG-UI streaming and the Store-to-Agent-to-Products path.
- Build and run the scenario with the .NET 11 RC SDK, then run existing .NET 10 scenario tests after any shared package refresh.
- Document the experimental SDK/package prerequisites, AI configuration, safe demo prompts, startup steps, and known limitations.
- Verify that no secrets, conversation contents, or autonomous checkout/payment paths are introduced.

## Package and framework update strategy

The repository currently targets `net10.0`. Updating to the latest stable .NET 10 SDK patch should not change those target frameworks. Aspire project/package references currently include 13.4.2 in the inspected baseline; the repository-wide refresh target is Aspire 13.6.1, with all Aspire packages and AppHost SDKs moved together and every scenario validated. The Blazor AI package is a deliberate exception: use a .NET 11 RC and prerelease component package only in Scenario 18 while the feature remains experimental.

The refresh now pins the stable SDK at 10.0.402 and updates 17 existing scenario solutions,
including API migrations for MCP, A2A, and MAF. Their builds and existing tests pass.
SQL-vector scenarios 08 and 12 use EF Core 9.0.21 to remain compatible with
EFCore.SqlServer.VectorSearch 9.0.0; mixing that provider with EF 10 causes runtime
MissingMethodException failures. Their EF tools also remain on 9.0.21.
The 14-MAFDevUI dependency refresh is blocked by its local requirement for an unavailable
Azure best-practices tool. Its dependencies remain unchanged; a later CLI-only fix adds
scenario-local AppHost selection consistently across the repository.
See the root README's dependency baseline for the exact boundaries.

## Acceptance criteria

- Scenario 18 runs independently and leaves all existing scenarios' behavior unchanged.
- A natural-language prompt yields streamed, catalog-grounded product recommendations.
- Product search and details are performed by bounded server-side tools, not by model-invented data.
- Cart mutation cannot occur until the user approves the specific action; rejection performs no mutation.
- Checkout, payment, and order placement remain outside the agent tool set.
- Agent, Store, Products API, and SQL Server appear as separate, healthy Aspire resources.
- Conversations and secrets are not exposed in logs or traces.
- All package versions and SDK/framework choices are documented, and builds/tests pass for the scenario plus every scenario affected by shared dependency updates.

## Sources

- [Build Agentic UI with the new Blazor AI components](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/)
- [ASP.NET Core 11 release notes: experimental Blazor AI components](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#blazor)
- [Official .NET downloads](https://dotnet.microsoft.com/en-us/download/dotnet)
- [.NET Aspire releases](https://github.com/microsoft/aspire/releases)
- [AG-UI integration with Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/ui/ag-ui/)
- [Scenario 01: Semantic Search](../01-SemanticSearch/README.md)
- [Scenario 01 Products API](../01-SemanticSearch/src/Products/Endpoints/ProductEndpoints.cs)
- [Scenario 01 Blazor search page](../01-SemanticSearch/src/Store/Components/Pages/Search.razor)
