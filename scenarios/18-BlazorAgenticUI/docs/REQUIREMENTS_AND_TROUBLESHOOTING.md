# Scenario 18: requirements and troubleshooting

[Quick start](../README.md) | [Detailed setup](DETAILS.md) | [Code walkthrough](CODE_WALKTHROUGH.md) | [Screenshots](WALKTHROUGH.md)

## Requirements

| Requirement | Why it is needed | How to check |
|---|---|---|
| Repository cloned locally | Contains the AppHost, services, images, and scripts | Start from the `eShopLite` repository root. |
| .NET SDK `11.0.100-rc.2.26504.105` | Pinned by this scenario's `global.json`; required for the experimental Blazor package | From this scenario folder: `dotnet --version`. |
| Aspire CLI 13.6 or later | Orchestrates services and manages parameter secrets | `aspire --version`. Scenario packages use 13.6.1. |
| PowerShell 7 | Runs the cross-platform setup scripts | `pwsh --version`. |
| Running Docker or Podman | Hosts SQL Server 2025 in normal mode | `docker info` or `podman info`, using the configured runtime. |
| Available ports and a trusted development HTTPS certificate | Aspire dashboard and service endpoints | Use the links printed at startup; `dotnet dev-certs https --check` checks the certificate. |
| Internet access to NuGet, container registry, and Foundry | Package restore, SQL image download, embedding and chat requests | Proxy/firewall rules must allow the services your environment uses. |
| Foundry inference endpoint and API key | Server-side model authentication | Use the account's inference endpoint, not `/api/projects/...`. Do not paste keys into issues or screenshots. |
| Chat deployment supporting Responses, streaming, and function tools | Agent reasoning and catalog/UI tool calls | Default deployment name: `gpt-6-luna`; it must exist in your account. |
| Embeddings deployment | Builds the small product vector index | Default deployment name: `text-embedding-3-small`; it must exist in your account. |
| Model quota and permission to call both deployments | Startup embeddings and each chat/tool turn consume service capacity | Check deployment status and quota in Foundry. Availability is not guaranteed in every region/account. |

The normal application graph is **Store + Agent + Products + SQL Server**. Products
waits for SQL, initializes nine catalog products, and generates embeddings. Agent and
Store wait for their dependencies. Initial SQL image download and initialization can
take longer than later runs.

The root repository pins **.NET 10.0.402** for the other scenarios. Install that SDK
too if you intend to work on them. Installing .NET 11 does not satisfy the .NET 10 pin.
Do not change the root pin just to make Scenario 18 start.

### Model secrets

Run the secret script from the repository root:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 -ScenariosRoot .\scenarios\18-BlazorAgenticUI
```

| Secret key | Required value |
|---|---|
| `Parameters:AzureOpenAIEndpoint` | HTTPS inference base, for example `https://<account>.openai.azure.com/` or `https://<account>.services.ai.azure.com/`. |
| `Parameters:AzureOpenAIApiKey` | Key for that inference account, entered at the masked prompt. |
| `Parameters:AzureOpenAIDeploymentName` | Actual chat deployment name, default `gpt-6-luna`. |
| `Parameters:AzureOpenAIEmbeddingsDeploymentName` | Actual embedding deployment name, default `text-embedding-3-small`. |

An endpoint already ending in `/openai/v1/` is accepted without duplicating the suffix.
The scenario has an independent AppHost user-secrets store. Store never receives the
model key. User-secrets are a local development mechanism, not encrypted production storage.
`az login` is needed by the optional provisioning script, not a replacement for these keys.

For provisioning instructions, see the [shared Foundry guide](../../../README.md#shared-microsoft-foundry-models-and-local-secrets).
Provisioning can incur Azure charges and is not necessary if deployments already exist.
The scripts configure common models only, not every specialized dependency in the repository.

### Optional offline mode

The [offline instructions](DETAILS.md#run-the-offline-demo) use SQLite and a deterministic
chat fixture. That path does not need Foundry credentials or containers, but still needs
the scenario SDK and Aspire CLI. It is not natural-language model evaluation or semantic
search. Stop a current run before changing modes.

## Troubleshooting

Run the following commands from `scenarios\18-BlazorAgenticUI` unless noted otherwise.

| Symptom | Likely cause | What to do |
|---|---|---|
| Startup asks for SDK `10.0.402` rather than .NET 11 RC2 | Aspire selected another AppHost or the command ran outside this scenario | Run `aspire config get appHost.path`. It should report `src/eShopAppHost/eShopAppHost.csproj`. Use the explicit startup command below if needed. |
| `A compatible .NET SDK was not found` for .NET 11 | Pinned SDK is absent from the `dotnet` installation used by Aspire | Run `dotnet --list-sdks` and `dotnet --version` here. Install the pinned SDK globally or configure the same SDK root for the shell and its child processes. |
| Secret script reports Aspire exit code 7 | SDK/project evaluation may have failed before any model call | Read the SDK preflight message. Azure sign-in alone cannot fix local SDK resolution. See the optional `-DotNetRoot` setup in [detailed notes](DETAILS.md#configure-real-models). |
| Dashboard asks for missing parameters | Secrets were set for another AppHost or are missing | Rerun the script with this scenario's `-ScenariosRoot`. The AppHost launch profile must use Development to load local secrets. Never share the raw secrets file. |
| SQL does not start | Container runtime stopped, image pull blocked, or resource initialization still running | Start your runtime, inspect `aspire describe`, and inspect SQL logs. The first SQL initialization can take a few minutes. Do not delete the data volume as a first troubleshooting step. |
| Products does not become healthy | SQL connection or embedding initialization failed | Inspect Products logs. Verify embedding endpoint/key/deployment and quota. Startup intentionally fails instead of silently falling back to offline mode. |
| Model returns 401/403 | Wrong key/account, expired or revoked key, disabled key authentication, or denied access | Verify the key belongs to the endpoint and account policy permits this local key-auth sample. Do not weaken account policy just to bypass the failure. |
| Model returns deployment-not-found/404 | Endpoint or deployment name is wrong | Use the inference endpoint and actual deployment name, which can differ from the model family name. Do not use a Foundry project URL. |
| Model returns 429 | Rate limit or quota exhaustion | Check model quota and service response, reduce concurrent requests, and retry later. The app does not substitute another model. |
| Tool calls fail with a Chat Completions reasoning error | Old/custom code uses `/chat/completions` for `gpt-6-luna` tools | Current `Agent/Program.cs` uses Responses. Keep that registration; do not silently disable reasoning. |
| Chat shows activity but no answer yet | Model reasoning, embedding search, or tool continuation is still active | Wait for the current turn before sending another prompt. Check Agent/Products health if it persists. Model latency is variable. |
| Chat displays a failure notice | Agent transport or provider request failed | Inspect Agent and Products operational logs. Check the cart before repeating an action; do not assume a failed continuation undid a confirmed addition. |
| No cart item appears after asking to add | Human approval is pending, the request was rejected, or the product could not be verified | Click Approve only for the intended product. Reject must leave the cart unchanged. A product lookup failure disables approval. |
| Cart disappears after a fresh session/restart | Cart is scoped to the Blazor circuit | Expected: there is no durable cart or checkout. Do not use this sample for real orders. |
| `MSB3026`, file-in-use, or locked assemblies during build | Aspire still holds the running service binaries | Run `aspire stop`, then build. Prefer resource rebuild commands while using Aspire. Do not delete `bin`/`obj` to work around a live process. |
| Browser links show a port from an earlier run | Service ports are dynamically assigned | Reopen Store from the current dashboard. Screenshot ports are illustrative, not fixed configuration. |
| Browser warns about an untrusted local HTTPS certificate | Local development certificate is not trusted | Review and establish trust through your normal development certificate workflow. Do not bypass browser security warnings. |

### Explicit startup and health

```powershell
aspire start --apphost .\src\eShopAppHost\eShopAppHost.csproj --non-interactive
aspire wait products --non-interactive
aspire wait agent --non-interactive
aspire wait store --non-interactive
```

For foreground operation use `aspire run`. For status and targeted logs:

```powershell
aspire describe
aspire logs products
aspire logs agent
aspire logs sql
```

Review output locally before sharing. Do not publish dashboard login tokens, keys,
connection strings, raw resource environment values, or private conversation content.
Do not enable sensitive-data logging for real shopper conversations.

### Stop, build, and check

```powershell
aspire stop
dotnet build .\src\eShopLite-BlazorAgenticUI.slnx
dotnet test .\src\Scenario.Tests\Scenario.Tests.csproj
dotnet format .\src\eShopLite-BlazorAgenticUI.slnx --verify-no-changes
```

These tests do not require a paid model. See [validation scope](DETAILS.md#local-validation)
for what was also checked with real Foundry models.

## Scope and safety

This is a local, unauthenticated experimental sample, not a production service.
Do not expose `/agent` publicly. There are no payment, order, checkout, account-change,
stock, discount, weight, or capacity capabilities. Model text is not authoritative
business data. Prices and IDs come from the catalog, and cart decisions come from the UI.
See [implementation decisions](DETAILS.md#scope-and-implementation-decisions) for deferred features.
