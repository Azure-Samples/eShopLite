# Agent Framework DevUI

This .NET 10 scenario combines a Blazor Store, a SQL-backed Products API, and
Microsoft Agent Framework agents. The checkout workflow runs StockAgent followed
by DiscountAgent. DevUI exposes the three registered agents and
`CheckoutWorkflowSequential` for local debugging.

## Prerequisites

- The repository's .NET SDK 10.0.402 and Aspire CLI 13.6.1 or later.
- Docker or Podman, running Linux containers for SQL Server 2025.
- An existing Azure OpenAI-compatible resource with chat and embedding deployments.
  The sample defaults are `gpt-5-mini` and `text-embedding-3-small`; configure the
  actual deployment names in your resource.

No Azure provisioning or deployment is required to build or run the offline tests.

## Configure and run

From the repository root, set these AppHost secrets with the Aspire CLI:

```powershell
aspire secret set Parameters:AzureOpenAIEndpoint "https://<resource>.openai.azure.com/" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIApiKey "<api-key>" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIDeploymentName "<chat-deployment>" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIEmbeddingsDeploymentName "<embedding-deployment>" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
aspire start --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj --non-interactive
aspire wait store --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj --non-interactive --timeout 180
aspire describe --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj --non-interactive
```

Use the Store URL reported by Aspire, then append `/devui`. Ports are dynamic.
The Products API is exposed at `/api/Product`.
Local agents are the default, with no Foundry project connection required.
`AzureOpenAIDeploymentName` takes precedence over the legacy
`AI_ChatDeploymentName` setting.

The Store uses the OpenAI-compatible `/openai/v1/responses` API for model calls.
This supports reasoning models with function tools, including models that reject
that combination through Chat Completions. It uses `OpenAIClient` directly because
the selected Azure OpenAI SDK's Responses wrapper is binary-incompatible with
the newer OpenAI SDK required by Agent Framework. Products retains the Azure SDK
for embeddings and its tool-free search summary.

In Development, DevUI also exposes `GET /v1/entities`, `POST /v1/responses`, and
conversation endpoints. A Responses request selects an entity using
`metadata.entity_id`, not the `model` field:

```json
{
  "input": "GOLD membership, subtotal $100. Calculate the membership discount.",
  "metadata": { "entity_id": "DiscountAgent" },
  "stream": false
}
```

Select `CheckoutWorkflowSequential` to exercise both checkout agents. Stock
availability in the checkout prompt remains a demonstration, not inventory
reservation. StockAgent also has a Products API stock-search tool.
DevUI is loopback-only by default. DevUI, Responses, and conversation routes are
not mapped outside Development.

Stop the app when finished:

```powershell
aspire stop --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj --non-interactive
```

### Local Azure CLI identity instead of an API key

Both Store and Products select `DefaultAzureCredential` when the API key is empty.
An existing Azure CLI sign-in must already have model-inference access to the
resource. This does not grant permissions or create a login.

To validate with identity without replacing saved secrets, run the following in a
dedicated PowerShell window. It copies AppHost parameters into that process only,
overrides the key with an empty value, and uses the tenant of the current Azure CLI
account rather than any stale inherited `AZURE_TENANT_ID`:

```powershell
$project = ".\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj"
$raw = (dotnet user-secrets list --project $project --json) -join "`n"
$parameters = $raw.Substring($raw.IndexOf('{')) | ConvertFrom-Json
foreach ($parameter in $parameters.PSObject.Properties) {
    if ($parameter.Name.StartsWith("Parameters:")) {
        [Environment]::SetEnvironmentVariable(
            $parameter.Name.Replace(":", "__"), [string]$parameter.Value, "Process")
    }
}
$env:Parameters__AzureOpenAIApiKey = ""
$env:AZURE_TOKEN_CREDENTIALS = "AzureCliCredential"
$env:AZURE_TENANT_ID = az account show --query tenantId --output tsv --only-show-errors
aspire start --apphost $project --isolated --non-interactive
aspire wait store --apphost $project --non-interactive --timeout 180
```

Do not print or commit the copied parameters. Stop the AppHost when finished and
close that PowerShell window to discard the process overrides. This is a local
validation configuration, not a production authentication recommendation.

### Optional Foundry agents

To use existing Foundry prompt agents instead of local chat-client agents, set
these additional AppHost secrets:

```powershell
aspire secret set AgentSettings:UseFoundryAgents "true" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
aspire secret set ConnectionStrings:microsoftfoundryproject "https://<resource>.services.ai.azure.com/api/projects/<project>" --apphost .\scenarios\14-MAFDevUI\eShopAppHost\eShopAppHost.csproj
```

The project must already contain named agents `StockAgent`, `DiscountAgent`, and
`TriageAgent`, configured with suitable models and instructions. Foundry mode uses
`DefaultAzureCredential`, including an existing Azure CLI sign-in locally. It does
not create agents or assign permissions. The stock agent's remote tool declaration
must match the local `SearchProductStock` function.
The Products service still needs the Azure OpenAI embedding configuration above.
An absent or non-HTTPS project endpoint fails configuration validation.
Set `AgentSettings:UseFoundryAgents` back to `false` to return to local agents.

## Dependency baseline and compatibility

| Component | Selected version |
|---|---|
| Target framework / SDK | .NET 10 / 10.0.402 |
| Aspire SDK and stable integrations | 13.6.1 |
| Aspire Azure OpenAI integration | 13.6.1-preview.1.26506.6 |
| Agent Framework core, OpenAI, workflows | 1.24.0 |
| DevUI, hosting, Foundry integration | 1.24.0-preview.261006.1 |
| OpenAI hosting transport | 1.24.0-alpha.261006.1 |
| Microsoft.Extensions.AI and VectorData abstractions | 10.11.0 |
| EF Core InMemory | 10.0.13 |
| OpenTelemetry exporters / instrumentation | 1.19.1 / 1.19.0 |
| Azure.Identity | 1.21.0 |

These coordinated versions match the repository baseline, not an unconditional
upgrade to every newest release. DevUI, Foundry, and OpenAI hosting retain
prerelease dependencies because their integrations are not stable in this
baseline. `Microsoft.Agents.AI.Foundry` replaces the legacy
`Microsoft.Agents.AI.AzureAI` package. The Foundry SDK brings the corresponding
Azure AI Projects and OpenAI extension prereleases transitively.

AppHost explicitly selects MessagePack 2.5.303 and PKCS 10.0.13 to patch
vulnerable transitive versions without changing MessagePack's major version.
Products also selects PKCS 10.0.13. No advisories were reported for the resolved
direct or transitive graph during the October 27, 2026 validation.

## Validation

Run from the repository root:

```powershell
dotnet build .\scenarios\14-MAFDevUI\eShopLite-Aspire-DevUI.slnx --verbosity quiet --nologo
dotnet test .\scenarios\14-MAFDevUI\eShopLite-Aspire-DevUI.slnx --verbosity quiet --nologo
dotnet list .\scenarios\14-MAFDevUI\eShopLite-Aspire-DevUI.slnx package --vulnerable --include-transitive
```

The migration passed the solution build, eight Products tests, and nineteen Store
tests. Store tests cover configuration validation, both agent registration modes,
streamed partial JSON, the real Store host's DevUI entity directory and Responses
transport, the actual model Responses client with API-key and identity configuration,
inference endpoint validation, sequential checkout without fallback, and Production
endpoint exclusion. The automated suite makes no Azure model calls.

An isolated Aspire run reached healthy SQL, Products, and Store resources.
Store `/`, `/devui`, and `/v1/entities` returned HTTP 200; the Products API
returned nine seeded products, all nine embedded successfully with zero embedding
failures. Semantic search returned matching products and a generated summary.
Using existing Azure CLI identity, live stock function calls matched the Products
API's inventory results. Both buffered and SSE Responses workflow requests completed.
The actual checkout orchestrator also completed Gold, Silver, and Normal checkouts
against the live model: discounts of 20, 10, and 0 on a subtotal of 100, both agent
steps successful, workflow diagrams present, and no deterministic fallback.

After the user replaced the saved API key, the same isolated app also passed
key-based validation without identity overrides: chat, all nine embeddings,
semantic search with a generated summary, all three sequential checkout tiers,
and SSE stock function invocation. The earlier HTTP 401 is resolved.

Existing Foundry-hosted agent execution remains **unvalidated** because no Foundry
project was configured. No cloud resources, agent definitions, permissions, or
OS-level environment settings were changed. The user updated the saved key locally;
validation did not print or persist it elsewhere.

If a model request returns HTTP 401, verify that the configured resource still exists,
its endpoint and API key match, and the selected identity has access. A
tenant error requires checking the inherited `AZURE_TENANT_ID` against the current
Azure CLI account. Changing package versions will not repair credentials or tenant
configuration. Then repeat the Responses request.
Do not treat the checkout's existing deterministic fallback discounts or a healthy
Store page as evidence that model authentication succeeded.
