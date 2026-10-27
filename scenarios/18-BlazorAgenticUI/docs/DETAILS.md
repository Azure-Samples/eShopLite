# Scenario 18: detailed setup and implementation notes

[Quick start](../README.md) | [Requirements and troubleshooting](REQUIREMENTS_AND_TROUBLESHOOTING.md) | [Code walkthrough](CODE_WALKTHROUGH.md) | [Screenshot walkthrough](WALKTHROUGH.md)

An experimental, independently runnable eShopLite scenario built from the product catalog
and search boundaries in [01-SemanticSearch](../../01-SemanticSearch/README.md).
It demonstrates the components introduced in
[Build Agentic UI with the new Blazor AI components](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/).

## What is implemented

- `ChatPage` and `UIAgent` stream a conversation from a separate agent service over AG-UI.
- Conversation status shows model/tool activity and safe, actionable failures without exposing provider diagnostics.
- Microsoft Agent Framework exposes bounded, read-only `search_products` and `get_product` tools.
- Source-generated `[ToolBlock]` types render authoritative catalog results as product cards.
- A frontend `UIActionBlock` pauses a cart request until the user clicks Approve or Reject.
- Cart prices are reloaded from the catalog after approval; cart state belongs to one Blazor circuit.
- Aspire orchestrates Store, Agent, Products, and SQL Server, with service discovery and telemetry.
- An explicitly labeled offline demo mode exercises the same AG-UI and UI-action pipeline
  with deterministic tool calls and SQLite, without calling a model.

The original nine products and images are reused from scenario 01. The catalog is read-only.
There is no checkout, payment, order placement, persistent cart, authentication, or cloud
deployment template in this scenario. It is a local development sample, not a public service.
Do not expose its unauthenticated agent endpoint to the internet.

## Prerequisites

- .NET SDK `11.0.100-rc.2.26504.105`, pinned in this scenario's `global.json`.
- Aspire CLI 13.6 or later.
- Docker or Podman running for normal mode (SQL Server 2025).
- A Microsoft Foundry chat deployment supporting tools and streaming, and an embeddings deployment.

Only this scenario targets .NET 11 RC2. Existing scenarios are not retargeted.
The Blazor AI package is `0.1.0-preview.1.26466.103`, built against .NET 11 RC1 and
verified here with RC2. MAF is `1.24.0`, its AG-UI hosting integration is
`1.24.0-preview.261006.1`, AGUI.Client is `1.0.1`, and Aspire is `13.6.1`.
Preview APIs may change. The project files record the remaining exact versions.

Install the SDK globally to use the normal `dotnet` and Aspire commands. Alternatively,
if using the user-local SDK installation, set the environment in the terminal before
running the following commands:

```powershell
$env:DOTNET_ROOT = Join-Path $env:LOCALAPPDATA 'eShopLite\dotnet11'
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
```

Otherwise use a normal .NET 11 RC2 SDK installation on `PATH`. Run SDK commands from this
scenario directory so the scenario-local `global.json` is selected.

## Configure real models

From the repository root:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 -ScenariosRoot .\scenarios\18-BlazorAgenticUI
```

The script checks SDK resolution from each AppHost directory before requesting the API
key or writing secrets. Aspire exit code 7 can mean the SDK in `global.json` is unavailable,
even when the AppHost file exists; Azure sign-in does not fix local SDK resolution.
For an SDK installed outside the system `PATH`, use:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 `
    -ScenariosRoot .\scenarios\18-BlazorAgenticUI `
    -DotNetRoot (Join-Path $env:LOCALAPPDATA 'eShopLite\dotnet11')
```

`-DotNetRoot` applies only to the script's child commands and restores the caller's
environment afterward. It does not install an SDK or change system settings.

The script discovers this AppHost and sets:

| AppHost parameter | Value |
|---|---|
| `Parameters:AzureOpenAIEndpoint` | Base HTTPS Azure OpenAI inference endpoint, such as `https://<account>.openai.azure.com/` |
| `Parameters:AzureOpenAIApiKey` | API key, entered at the masked prompt |
| `Parameters:AzureOpenAIDeploymentName` | Chat deployment name, default `gpt-6-luna` |
| `Parameters:AzureOpenAIEmbeddingsDeploymentName` | Embeddings deployment name, default `text-embedding-3-small` |

Use the inference endpoint, not a Foundry project URL. The server-side clients normalize
the OpenAI-compatible `/openai/v1/` path, including when that suffix is already present.
Deployment names must exist on that account; `gpt-6-luna` availability is not assumed.
The endpoint and API key are not injected into Store.

This scenario has its own `UserSecretsId`; it does not share another sample's secret store.
To provision shared models first, see the root
[Foundry setup instructions](../../../README.md#shared-microsoft-foundry-models-and-local-secrets).
The setup scripts now include this AppHost automatically.

## Run with real models and SQL Server

```powershell
cd .\scenarios\18-BlazorAgenticUI
aspire start
```

This scenario's `aspire.config.json` selects its own AppHost, so a bare `aspire start`
does not inherit another scenario selected in a parent configuration. Use `aspire run`
instead for a foreground session, or use the explicit form for automation:

```powershell
aspire start --apphost .\src\eShopAppHost\eShopAppHost.csproj --non-interactive
aspire wait products --non-interactive
aspire wait agent --non-interactive
aspire wait store --non-interactive
```

Open Store from the Aspire dashboard. Products creates and seeds the SQL database,
then builds its in-memory vector index using the configured embedding model. Any
connection or embedding failure prevents startup rather than switching to demo behavior.
The agent uses the configured chat deployment and real tool calls through the OpenAI
Responses API. This is required for `gpt-6-luna` function tools with reasoning enabled;
Chat Completions rejects that combination. The SDK's Responses adapter is experimental,
so its `OPENAI001` diagnostic is suppressed only around this registration. Reasoning
is not silently disabled and the model is not replaced.

If startup unexpectedly requests the root SDK `10.0.402` while you are in this scenario,
check `aspire config get appHost.path`: an inherited setting may be selecting a different
AppHost. The expected value here is `src/eShopAppHost/eShopAppHost.csproj`; the scenario SDK
is .NET 11 RC2. The other stable scenarios still require the root SDK independently.

The local vector search is deliberately small: product embeddings are held in process
and cosine similarity ranks at most nine catalog entries. A restart rebuilds the index.
Price and result limits are applied in application code rather than entrusted to the model.

## Run the offline demo

```powershell
cd .\scenarios\18-BlazorAgenticUI
aspire start --apphost .\src\eShopAppHost\eShopAppHost.csproj --non-interactive -- --Demo=true
aspire wait store --non-interactive
```

This does not need secrets or a container runtime. The banner explicitly identifies the
fixture. Any ordinary prompt triggers a fixed keyword search for `camping`; a prompt
containing `cart` requests approval to add product 9 (Camping Tent). It is not natural
language interpretation and does not prove that a Foundry deployment works.
Semantic search is unavailable in this mode, and the API returns 503 if requested directly.

Stop the current run before switching between demo and real mode:

```powershell
aspire stop --apphost .\src\eShopAppHost\eShopAppHost.csproj --non-interactive
```

## Suggested walkthrough

1. Ask `Find camping equipment under $100` and inspect the streamed catalog cards.
2. Ask `Add the tent to my cart`. Verify the cart is still empty while approval is pending.
3. Reject the request. The cart must stay empty.
4. Ask again and approve. The cart shows one Camping Tent at the catalog price of $99.99.
5. Open a separate browser session. It must not inherit another circuit's cart or conversation.

For the real model also try `Show rain protection under $60`, an unavailable product,
and a request about product weight. The catalog has no weight or tent-capacity data,
so the assistant must not invent those facts.

## Build and tests

From this scenario directory:

```powershell
dotnet build .\src\eShopLite-BlazorAgenticUI.slnx
dotnet test .\src\Scenario.Tests\Scenario.Tests.csproj
```

The tests include price/result bounds, missing model behavior, cart rejection and isolation,
catalog failures, AG-UI HTTP streaming through the real MAF server, source-generated product
block deserialization, and UI actions that cannot execute until a decision is supplied.
Protocol tests use an explicit offline chat client, not a Foundry deployment. A separate
mock-provider test verifies that real-mode wiring sends streaming requests and catalog
tools to `/openai/v1/responses` with the configured deployment. Error-rendering coverage
checks that transport failures produce a visible notice without provider details.

## Local validation

The scenario was validated locally with SQL Server 2025, `gpt-6-luna`, and
`text-embedding-3-small`, not only with the offline fixture. The browser walkthrough
confirmed streamed catalog cards, a rain-jacket recommendation below $60, an empty cart
after rejection, and one Camping Tent at $99.99 after approval. The model also reported
that weight and person capacity are absent from the catalog instead of inventing them.
All resources became healthy again after restarting against the existing SQL volume.

The scenario build and formatting check pass, and all 29 automated tests pass.
These results establish the tested local integration, not a guarantee of model behavior
for every prompt, deployment, region, or future package version.

## Scope and implementation decisions

The architecture follows the [implementation plan](../IMPLEMENTATION_PLAN.md), with these
deliberate first-version choices:

- All projects in this isolated scenario use the same .NET 11 SDK to simplify build/run.
- The original catalog, SQL-backed API, and search boundary are retained, not a wholesale
  copy of the baseline's CRUD, checkout, generated Azure infrastructure, or database files.
- Cart approval uses `RegisterUIAction` / `UIActionBlock`, because the cart belongs to the
  Blazor circuit. `FunctionApprovalBlock` is for approval-required backend tools and is not
  needed for this local cart action.
- Typed shared/predictive state, rich Markdown parsing, durable conversations, and production
  authentication/deployment are not implemented. The card result and cart already demonstrate
  application-owned UI without adding an unnecessary state protocol.
- Logs/traces are operational only. Prompt bodies, keys, and tool results are not explicitly
  logged; verbose HTTP URL logging and agent information logs are disabled. Do not enable
  sensitive-data logging when using real conversations.

The component preview is not a production support commitment. A successful offline run
does not establish real-model quality, regional availability, quota, or cloud deployability.
