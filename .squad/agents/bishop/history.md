# Bishop History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Stack: .NET 9 APIs, Aspire orchestration, Azure AI integrations

## Learnings
- Initial team setup prioritizes stable contracts between APIs and scenario-specific infrastructure.

## 2026-06-06 — Scenario 01-SemanticSearch Reference Pattern

### VectorData In-Memory Provider

**Package chosen:** `CommunityToolkit.VectorData.InMemory` **1.0.0-preview.3**

- Namespace: `CommunityToolkit.VectorData.InMemory`
- Class used: `InMemoryVectorStore` (same name as the SK connector; drop-in swap)
- Only `using` change: replace `using Microsoft.SemanticKernel.Connectors.InMemory;` with `using CommunityToolkit.VectorData.InMemory;`
- Requires `Microsoft.Extensions.VectorData.Abstractions >= 10.5.2` (bump `VectorEntities.csproj` from 9.7.0 → **10.6.0**)
- Requires `Microsoft.Extensions.AI.Abstractions >= 10.5.2` (bump `Products.csproj` from 10.0.1 → **10.6.0**)
- Removed transitive SK dep on `Newtonsoft.Json`: replaced `JsonConvert.SerializeObject` with `System.Text.Json.JsonSerializer.Serialize` in `MemoryContext.cs`

### Aspire Parameter Names & User-Secrets Keys

Four Aspire parameters, all defined in `eShopAppHost/Program.cs`:

| Aspire Parameter Name                  | Secret? | Purpose                    |
|----------------------------------------|---------|----------------------------|
| `AzureOpenAIEndpoint`                  | No      | Azure OpenAI endpoint URL  |
| `AzureOpenAIApiKey`                    | **Yes** | API key                    |
| `AzureOpenAIDeploymentName`            | No      | Chat deployment name       |
| `AzureOpenAIEmbeddingsDeploymentName`  | No      | Embeddings deployment name |

**User-secrets keys** (set in `eShopAppHost` project with `dotnet user-secrets set`):
```
Parameters:AzureOpenAIEndpoint             https://<resource>.openai.azure.com/
Parameters:AzureOpenAIApiKey               <your-api-key>
Parameters:AzureOpenAIDeploymentName       gpt-5-mini
Parameters:AzureOpenAIEmbeddingsDeploymentName  text-embedding-3-small
```

**AppHost snippet (run mode):**
```csharp
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey   = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment       = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

products
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);
```

**Products/Program.cs consumption snippet:**
```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using System.ClientModel;

var endpoint   = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey     = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploy = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-5-mini";
var embDeploy  = builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-3-small";

if (!string.IsNullOrEmpty(endpoint))
{
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploy).AsIChatClient());
    builder.Services.AddEmbeddingGenerator(aoaiClient.GetEmbeddingClient(embDeploy).AsIEmbeddingGenerator());
}
```

### API-Shape Gotchas

- `ApiKeyCredential` is in `System.ClientModel` (NOT `Azure.Core`) — use `using System.ClientModel;`
- In `Microsoft.Extensions.AI.OpenAI` 10.6.0 the extension methods are **`AsIChatClient()`** and **`AsIEmbeddingGenerator()`** (not `AsChatClient`/`AsEmbeddingGenerator` which were removed)
- Call them on the sub-clients: `aoaiClient.GetChatClient(deployment).AsIChatClient()` and `aoaiClient.GetEmbeddingClient(deployment).AsIEmbeddingGenerator()`
- `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3 pin: requires `Microsoft.Extensions.AI.Abstractions >= 10.5.2`, which forced bumping that dep
- SK OTel switches (`Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive`, meter/source wildcards) were removed from `eShopServiceDefaults/Extensions.cs`

### Canonical Package Version List (net10 reference pattern)

| Package                                        | Version                            |
|------------------------------------------------|------------------------------------|
| Aspire.AppHost.Sdk                             | 13.0.1                             |
| Aspire.Hosting.AppHost                         | 13.0.1                             |
| Aspire.Hosting.Azure.ApplicationInsights       | 13.0.1                             |
| Aspire.Hosting.Azure.CognitiveServices         | 13.0.1                             |
| Aspire.Hosting.SqlServer                       | 13.0.1                             |
| Aspire.Azure.AI.OpenAI                         | 13.0.1-preview.1.25575.3           |
| Aspire.Microsoft.EntityFrameworkCore.SqlServer | 13.0.1                             |
| CommunityToolkit.VectorData.InMemory           | **1.0.0-preview.3**                |
| Microsoft.Extensions.AI.Abstractions          | **10.6.0**                         |
| Microsoft.Extensions.AI.OpenAI                | **10.6.0**                         |
| Microsoft.Extensions.VectorData.Abstractions  | **10.6.0**                         |
| Microsoft.EntityFrameworkCore.InMemory        | 10.0.0 (tests only)                |

## 2026-06-06 — Scenario 05-deepseek Modernization (sk-05-chat fulfilled)

### What Changed

- All projects: net9.0 → **net10.0**
- **eShopAppHost.csproj**: `Aspire.AppHost.Sdk` 9.0.0 → 13.0.1; removed `Aspire.Hosting.Azure`; all `Aspire.Hosting.*` 9.2.1 → 13.0.1
- **eShopServiceDefaults.csproj**: all OTel packages 1.11.x → 1.14.0; `Azure.Monitor.OpenTelemetry.AspNetCore` 1.3.0-beta.3 → 1.4.0; `Microsoft.Extensions.*` 9.x → 10.0.0
- **Products.csproj**: removed `Microsoft.SemanticKernel` 1.47.0 (chat), `Microsoft.SemanticKernel.Connectors.InMemory` 1.47.0-preview, all `Microsoft.KernelMemory.*` 0.98.x (5 packages), `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0; added `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3, `Microsoft.Extensions.AI.OpenAI` 10.6.0, `Microsoft.Extensions.AI.Abstractions` 10.6.0; bumped `Aspire.Azure.AI.OpenAI` 9.2.1-preview → 13.0.1-preview.1.25575.3, `Aspire.Microsoft.EntityFrameworkCore.SqlServer` 9.2.1 → 13.0.1
- **VectorEntities.csproj**: `Microsoft.Extensions.VectorData.Abstractions` 9.0.0-preview → **10.6.0**
- **VectorEntities/ProductVector.cs**: attributes `[VectorStoreRecordKey]`→`[VectorStoreKey]`, `[VectorStoreRecordData]`→`[VectorStoreData]`, `[VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]`→`[VectorStoreVector(384)]`
- **eShopAppHost/Program.cs**: replaced connection-string approach with **7 explicit Aspire parameters**: 4 for AOAI (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey`(secret), `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`) + 3 for DeepSeek (`DeepSeekEndpoint`, `DeepSeekApiKey`(secret), `DeepSeekDeploymentName`); all passed via `WithEnvironment` to Products; `IsPublishMode` branch kept for Azure provisioning
- **Products/Program.cs**: removed all KernelMemory + SK + `OpenAI.Embeddings.EmbeddingClient` / `OpenAI.Chat.ChatClient` references; replaced with MEAI `IChatClient` (keyed) + `IEmbeddingGenerator`; DeepSeek registered as `AddKeyedSingleton<IChatClient>("chatClientDeepSeekR1", ...)`
- **Products/Memory/MemoryContext.cs**: `using Microsoft.SemanticKernel.Connectors.InMemory` → `using CommunityToolkit.VectorData.InMemory`; removed `IKernelMemory` parameter; `ChatClient?` → `IChatClient?`; `EmbeddingClient?` → `IEmbeddingGenerator<string, Embedding<float>>?`; `IVectorStoreRecordCollection<int,ProductVector>` → `VectorStoreCollection<int,ProductVector>`; `CreateCollectionIfNotExistsAsync` → `EnsureCollectionExistsAsync`; `GenerateEmbeddingAsync(…).Value.ToFloats()` → `GenerateVectorAsync(…).ToArray()`; `VectorizedSearchAsync(query, options)` → `SearchAsync(query, top: 3)`; `CompleteChatAsync(…).Value.Content[0].Text` → `GetResponseAsync(…).Text`; `new SystemChatMessage/UserChatMessage` → `new ChatMessage(ChatRole.System/User, …)`; `Newtonsoft.Json.JsonConvert` → `System.Text.Json.JsonSerializer`; DeepSeek-R1 `<think>…</think>` parsing preserved

### DeepSeek-R1 Chat Client

**Client chosen:** `Microsoft.Extensions.AI.OpenAI` **10.6.0** — same package as AOAI  
**Pattern:**  
- No API key → `new AzureOpenAIClient(endpoint, new DefaultAzureCredential()).GetChatClient(deployment).AsIChatClient()`  
- API key present → `new OpenAIClient(new ApiKeyCredential(key), new OpenAIClientOptions { Endpoint = uri }).GetChatClient(deployment).AsIChatClient()`  

This mirrors the original code's approach (DeepSeek is on an OpenAI-compatible Azure endpoint) but uses MEAI `IChatClient` instead of SDK-native `ChatClient`.

### Keyed Registration Pattern (two chat clients)

Since both AOAI and DeepSeek chat clients must coexist:
```csharp
builder.Services.AddKeyedSingleton<IChatClient>("chatClientOpenAI",
    aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());

builder.Services.AddKeyedSingleton<IChatClient>("chatClientDeepSeekR1",
    deepseekClient.GetChatClient(deepseekDeploymentName).AsIChatClient());
```
Retrieved in MemoryContext via `sp.GetKeyedService<IChatClient>(key)`.

### User-Secrets Keys (eShopAppHost project)

```
Parameters:AzureOpenAIEndpoint                  https://<resource>.openai.azure.com/
Parameters:AzureOpenAIApiKey                    <your-api-key>
Parameters:AzureOpenAIDeploymentName            gpt-41-mini
Parameters:AzureOpenAIEmbeddingsDeploymentName  text-embedding-ada-002
Parameters:DeepSeekEndpoint                     https://<resource>.eastus.models.ai.azure.com/
Parameters:DeepSeekApiKey                       <your-deepseek-key>
Parameters:DeepSeekDeploymentName               DeepSeek-R1
```

### Build / Test

- `dotnet build eShopLite-Aspire-DeepSeekR1.slnx` → **Build succeeded** (0 errors, NU1902 vulnerability warnings only — same as scenario 01)
- `dotnet test eShopLite-Aspire-DeepSeekR1.slnx` → **No test projects** (Build succeeded, 0 tests)
- `grep Microsoft.SemanticKernel` → **0 results**



### What Changed

- **Products.csproj**: removed `Microsoft.SemanticKernel.Connectors.InMemory` 1.68.0-preview, `Aspire.Azure.AI.Inference`, `Microsoft.VisualStudio.Web.CodeGeneration.Design`; added `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3 + `Microsoft.Extensions.AI.OpenAI` 10.6.0; bumped `Microsoft.Extensions.AI.Abstractions` 10.0.1 → 10.6.0.
- **VectorEntities.csproj**: `Microsoft.Extensions.VectorData.Abstractions` 9.7.0 → 10.6.0.
- **AgentServices.csproj**: `Microsoft.Extensions.AI` / `Abstractions` 10.0.1 → 10.6.0; `Microsoft.Extensions.DependencyInjection.Abstractions` + `Logging.Abstractions` 10.0.0 → 10.0.8 (required by 10.6.0 chain).
- **Store.csproj**: added `Microsoft.Extensions.AI.OpenAI` 10.6.0.
- **Products/Memory/MemoryContext.cs**: `using Microsoft.SemanticKernel.Connectors.InMemory` → `using CommunityToolkit.VectorData.InMemory`; `Newtonsoft.Json.JsonConvert.SerializeObject` → `System.Text.Json.JsonSerializer.Serialize`.
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel switch (`Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive`), `AddMeter("Microsoft.SemanticKernel.*")`, `AddSource("Microsoft.SemanticKernel.*")`; kept `*Microsoft.Agents.AI` meter and agent sources.
- **eShopAppHost/Program.cs**: replaced `microsoftfoundry` connection string with 4 explicit parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey` (secret), `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`); kept `microsoftfoundryproject` connection string for Azure AI Foundry project SDK; kept `IsPublishMode` Azure OpenAI provisioning branch.
- **Products/Program.cs**: replaced `AddAzureOpenAIClient` with direct `AzureOpenAIClient` construction from env vars + `AsIChatClient()` / `AsIEmbeddingGenerator()`.
- **Store/Program.cs**: same direct AOAI client pattern; removed `microsoftFoundryConnectionName` reference.
- **AgentServices/AgentServicesExtensions.cs**: `AddAgentSettings` now reads `AzureOpenAIEndpoint` from config instead of `GetConnectionString("microsoftfoundry")`.

### Scenario-14 Specific Quirk

`AgentServices.csproj` pinned `Microsoft.Extensions.DependencyInjection.Abstractions` and `Logging.Abstractions` at `10.0.0`. When bumping `Microsoft.Extensions.AI` to `10.6.0`, those became NU1605 downgrade errors. Fix: bump both to `10.0.8`.

### Tests

- `dotnet test eShopLite-Aspire-DevUI.slnx` → **9/9 passed** (Products.Tests: 8, Store.Tests: 1)
- `grep Microsoft.SemanticKernel` → **0 results**
- MAF agent architecture (AgentCheckoutOrchestrator, Workflows, DevUI) left intact.



## 2026-06-06 — Scenario 08-Sql2025 Modernization

### What Changed

- All projects: net9.0 → **net10.0**
- **eShopAppHost.csproj**: `Aspire.AppHost.Sdk` 9.0.0 → 13.0.1; all `Aspire.Hosting.*` → 13.0.1
- **eShopServiceDefaults.csproj**: all OTel packages 1.12.0 → 1.14.0; `Azure.Monitor.OpenTelemetry.AspNetCore` 1.3.0 → 1.4.0; `Microsoft.Extensions.*` 9.x → 10.0.0
- **Store.csproj**: OTel 1.12.0 → 1.14.0; added `System.Text.Json 10.0.0`
- **Products.csproj**: `Aspire.Azure.AI.OpenAI` 9.3.1-preview → 13.0.1-preview.1.25575.3; added `Microsoft.Extensions.AI.Abstractions 10.6.0` + `Microsoft.Extensions.AI.OpenAI 10.6.0`; kept `EFCore.SqlServer.VectorSearch 9.0.0-preview.2`
- **Products.Tests.csproj**: removed `Moq` (unused), removed `Aspire.Azure.AI.OpenAI` (not needed in tests), `MSTest` 3.8.3 → 4.0.2; **pinned `Microsoft.EntityFrameworkCore.InMemory` to `9.0.8`** (see note below)
- **eShopAppHost/Program.cs**: replaced connection-string + `AI_*` env vars with **4 explicit Aspire parameters** (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey` secret, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`); removed `openai` connection string; kept `IsPublishMode` Azure OpenAI provisioning branch
- **Products/Program.cs**: replaced `AddAzureOpenAIClient` (with AzureCliCredential) with direct `AzureOpenAIClient` construction; reads 4 explicit config keys; kept SQL Server vector workaround (`UseSqlServer(connectionString, o => o.UseVectorSearch())`)
- **Products/Endpoints/ProductApiActions.cs**: changed `GetProductById`, `UpdateProduct`, `DeleteProduct` return types from `Task<Results<T1,T2>>` → `Task<IResult>` (struct→interface fix for test `as` casts); made `ILogger<ProductApiActions>? logger = null` optional with null-safe call
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel switches and meter/trace wildcards (`Microsoft.SemanticKernel.*`)

### SQL 2025 Vector Search — Package Compatibility Note

**Critical**: `EFCore.SqlServer.VectorSearch 9.0.0-preview.2` has dependency `Microsoft.EntityFrameworkCore.SqlServer [9.0.0, )` (open-ended, no upper bound), so it technically allows EF Core 10. However at **runtime**, the VectorSearch binary references `AbstractionsStrings.ArgumentIsEmpty` from EF Core 9's Relational assembly — a method that was renamed/removed in EF Core 10.

**Symptom**: `System.MissingMethodException: Method not found: 'System.String Microsoft.EntityFrameworkCore.Diagnostics.AbstractionsStrings.ArgumentIsEmpty(System.Object)'` during model building when EF Core InMemory 10 is in the process.

**Fix for tests**: Pin `Microsoft.EntityFrameworkCore.InMemory` to **9.0.8** in Products.Tests, keeping all EF Core at version 9 inside the test process.

**Fix for production**: No change needed — Products.csproj uses `EFCore.SqlServer.VectorSearch 9.0.0-preview.2` + `EFCore.SqlServer.VectorSearch` brings SqlServer 9.0.x; no InMemory 10 is in the production binary. Works fine against SQL Server 2025.

### MSTest 4.x Breaking Changes

- `Assert.ThrowsExceptionAsync<T>` was **removed** — replace with `Assert.ThrowsAsync<T>`
- MEAI extension method `GenerateVectorAsync` validates its argument before dereferencing → throws `ArgumentNullException` on null client, NOT `NullReferenceException`
- `Results<T1,T2>` in ASP.NET Core 10 is a struct — `as` cast on struct fails (CS0039). Fix: return `Task<IResult>` from endpoint methods and use `Results.Ok()`/`Results.NotFound()` instead of `TypedResults.Ok()`/`TypedResults.NotFound()`

### Build / Test

- `dotnet build eShopLite-Sql2025.slnx` → **Build succeeded** (0 errors, NU1902 vulnerability warnings only)
- `dotnet test eShopLite-Sql2025.slnx` → **7/7 passed** (all in Products.Tests; SQL container not needed for unit tests)
- `grep Microsoft.SemanticKernel` → **0 results**



- **Agent projects** (`ResearcherAgent`, `InventoryAgent`, `PromotionsAgent`): simple ASP.NET Web API stubs with no SK dependency — only TFM upgrade and package version bumps needed.
- **A2A package**: `A2A 0.1.0-preview.2` is compatible with net10; no API breakage found.
- **`Swashbuckle.AspNetCore`**: bumped 6.6.2 → 8.1.1 for net10 compatibility (OpenAPI endpoint registration).
- **`Microsoft.AspNetCore.OpenApi`**: bumped 8.0.18 → 10.0.0 for the agent projects.

### Breaking Changes Encountered

- **`OpenAI.Chat.SystemChatMessage` / `UserChatMessage`** removed in MEAI: replaced with `new ChatMessage(ChatRole.System, …)` / `new ChatMessage(ChatRole.User, …)` using `Microsoft.Extensions.AI` types.
- **`EmbeddingClient.GenerateEmbeddingAsync` / `.ToFloats()`** replaced with `IEmbeddingGenerator<string,Embedding<float>>.GenerateVectorAsync(…).ToArray()`.
- **`ChatClient.CompleteChatAsync(messages).Value.Content[0].Text`** replaced with `IChatClient.GetResponseAsync(messages).Text`.
- **`Newtonsoft.Json.JsonConvert.SerializeObject`** in `MemoryContext.cs` replaced with `System.Text.Json.JsonSerializer.Serialize` (SK was pulling Newtonsoft as a transitive dep).
- **`Microsoft.VisualStudio.Web.CodeGeneration.Design`** 9.0.0 removed from Products.csproj (unused scaffold-time tool, incompatible with net10).
- **AppHost** had `Aspire.Hosting.Azure.CognitiveServices` and `products.WithReference(aoai)` in publish mode — kept the Aspire Azure OpenAI resource provisioning for `IsPublishMode` but removed the `.WithReference(aoai)` call (parameters flow via `WithEnvironment` now).

### Tests

- `dotnet test eShopLite-A2A.slnx` → **9/9 passed** (Products.Tests: 8, Store.Tests: 1)
- `grep Microsoft.SemanticKernel **/*.cs **/*.csproj` → **0 results**


## 2026-06-06 — Scenario 09-AzureAppService Modernization

### What Changed

- All projects: net9.0 → **net10.0**
- **eShopAppHost.csproj**: `Aspire.AppHost.Sdk` 9.0.0 → 13.0.1; `Aspire.Hosting.AppHost` 9.3.1 → 13.0.1; `Aspire.Hosting.Azure.CognitiveServices` 9.3.1 → 13.0.1; `Aspire.Hosting.Azure.AppService` 9.3.1-preview → 13.0.1-preview.1.25575.3
- **Products.csproj**: removed `Microsoft.SemanticKernel.Connectors.InMemory` 1.60.0-preview, `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0; kept + bumped `Aspire.Azure.AI.OpenAI` → 13.0.1-preview.1.25575.3; added `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3, `Microsoft.Extensions.AI.OpenAI` 10.6.0, `Microsoft.Extensions.AI.Abstractions` 10.6.0; bumped EF Core 9.0.7 → 10.0.0; OTel 1.12.0 → 1.14.0
- **VectorEntities.csproj**: `Microsoft.Extensions.VectorData.Abstractions` 9.7.0 → 10.6.0
- **eShopServiceDefaults.csproj**: all packages bumped (OTel 1.12.0 → 1.14.0, Azure.Monitor 1.3.0 → 1.4.0, Http.Resilience 9.7.0 → 10.0.0, ServiceDiscovery 9.3.1 → 10.0.0)
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel switch + `AddMeter("Microsoft.SemanticKernel.*")` + `AddSource("Microsoft.SemanticKernel.*")`; added `Azure.Experimental.EnableActivitySource` switch; enabled Azure Monitor exporter block
- **eShopAppHost/Program.cs**: replaced `products.WithReference(aoai)` + old env vars with 4 explicit parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey` secret, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`) via `WithEnvironment`; kept `IsPublishMode` branch for Azure OpenAI resource provisioning (sans `WithReference`)
- **Products/Program.cs**: removed `OpenAI.*` + `builder.AddAzureOpenAIClient`; added 4-parameter pattern with direct `AzureOpenAIClient` construction + `AsIChatClient()` / `AsIEmbeddingGenerator()`; preserved SQLite `AddDbContext<Context>` wiring
- **Products/Memory/MemoryContext.cs**: `using Microsoft.SemanticKernel.Connectors.InMemory` → `using CommunityToolkit.VectorData.InMemory`; `ChatClient?` → `IChatClient?`; `EmbeddingClient?` → `IEmbeddingGenerator<string, Embedding<float>>?`; `GenerateEmbeddingAsync(…).Value.ToFloats()` → `GenerateVectorAsync(…).ToArray()`; `CompleteChatAsync(…).Value.Content[0].Text` → `GetResponseAsync(…).Text`; `new SystemChatMessage/UserChatMessage` → `new ChatMessage(ChatRole.System/User, …)`; `Newtonsoft.Json.JsonConvert` → `System.Text.Json.JsonSerializer`
- **Products/Endpoints/ProductEndpoints.cs**: removed unused `using OpenAI.Embeddings;` and `using OpenAI.Chat;`
- **Store, DataEntities, SearchEntities, CartEntities**: TFM only (net9 → net10), package version bumps
- **Products.Tests.csproj**: MSTest 3.9.3 → 4.0.2; EF InMemory 9.0.7 → 10.0.0
- **Products.Tests/ProductApiActionsTests.cs**: `Assert.ThrowsExceptionAsync<T>` → `Assert.ThrowsAsync<T>` (MSTest 4 API rename)

### Scenario-09 Specific Notes

- **SQLite preserved**: `AddDbContext<Context>` with `UseSqlite(...)` kept intact; no migration to SQL Server
- **App Service deployment preserved**: `azure.yaml` + `infra/` untouched; `builder.AddAzureAppServiceEnvironment("appsvc")` retained in AppHost
- **IsPublishMode branch**: provisions Azure OpenAI resource (gpt-4.1-mini + text-embedding-ada-002 deployments) but does NOT wire it via `WithReference(aoai)` — explicit parameters carry the connectivity instead

### MSTest 4 Breaking Change

`Assert.ThrowsExceptionAsync<T>` removed in MSTest 4.0.2. Replacement: `Assert.ThrowsAsync<T>`.

### Build & Tests

- `dotnet build eShopLite-Aspire-AppService.slnx` → **succeeded** (0 errors; NU1902/NU1510 warnings only)
- `dotnet test eShopLite-Aspire-AppService.slnx` → **7/7 passed** (Products.Tests: 6, Store.Tests: 1)
- `grep Microsoft.SemanticKernel` → **0 results**


### Azure AI Search VectorData Provider Decision

**Decision:** `Microsoft.SemanticKernel.Connectors.AzureAISearch` **1.74.0-preview** is retained as the **one unavoidable SK-named dependency**.

**Research summary:**
- Searched NuGet for `CommunityToolkit.VectorData.AzureAISearch` and `Microsoft.Extensions.VectorData.AzureAISearch` — **no results**.
- The only Azure AI Search provider implementing `Microsoft.Extensions.VectorData` abstractions (`IVectorStore`, `VectorStoreCollection<TKey, TRecord>`) ships under the `Microsoft.SemanticKernel.Connectors.AzureAISearch` package name.
- This package has **no SK kernel/chat/agent dependency** at runtime — it is a pure VectorData connector that happens to live in the SK namespace for historical reasons.
- Bumped from 1.54.0-preview → **1.74.0-preview** (latest as of 2026-06-06).

### What Changed

- **eShopAppHost.csproj**: TFM net9 → net10; Aspire.AppHost.Sdk 9.2.1 → 13.0.1; all Hosting.* packages 9.3.0 → 13.0.1 (incl. Aspire.Hosting.Azure.Search).
- **eShopAppHost/Program.cs**: added 4 explicit parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey` secret, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`); wired to products via `WithEnvironment`; kept publish-mode block with Azure Search + OpenAI + AppInsights provisioning; removed `WithReference(aoai)` + old `WithEnvironment("AI_ChatDeploymentName", ...)` pattern.
- **Products.csproj**: TFM net9 → net10; removed `Aspire.Azure.AI.OpenAI` 9.3.0-preview + `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0; added `Aspire.Azure.AI.OpenAI` 13.0.1-preview.1.25575.3, `Microsoft.Extensions.AI.Abstractions` 10.6.0, `Microsoft.Extensions.AI.OpenAI` 10.6.0, `OpenTelemetry.Instrumentation.AspNetCore` 1.14.0; bumped Aspire.Azure.Search.Documents 9.3.0 → 13.0.1; Aspire.Microsoft.EntityFrameworkCore.SqlServer 9.3.0 → 13.0.1; EF Core 9.0.5 → 10.0.8; SK AzureAISearch 1.54.0-preview → 1.74.0-preview.
- **Products/Program.cs**: removed `OpenAI.Chat.*` / `OpenAI.Embeddings.*` + `builder.AddAzureOpenAIClient`; added 4-parameter MEAI pattern with `AzureOpenAIClient` + `AsIChatClient()` / `AsIEmbeddingGenerator()`; kept `builder.AddAzureSearchClient("azureaisearch")`.
- **Products/Memory/MemoryContext.cs**: `ChatClient` → `IChatClient`; `EmbeddingClient` → `IEmbeddingGenerator<string, Embedding<float>>`; `_embeddingClient.GenerateEmbeddingAsync(…).Value.ToFloats()` → `_embeddingGenerator.GenerateVectorAsync(…)` (returns `ReadOnlyMemory<float>` directly matching `ProductVector.Vector`); `JsonConvert.SerializeObject` → `JsonSerializer.Serialize`; `new SystemChatMessage`/`UserChatMessage` → `new ChatMessage(ChatRole.System/User, …)`; `CompleteChatAsync(…).Value.Content[0].Text` → `GetResponseAsync(…).Text`; `using Newtonsoft.Json` removed.
- **eShopServiceDefaults.csproj**: TFM net9 → net10; all packages bumped to match scenario-01 pattern (OTel 1.12.0 → 1.14.0, Azure.Monitor 1.3.0 → 1.4.0, Http.Resilience 9.5.0 → 10.0.0, ServiceDiscovery 9.3.0 → 10.0.0).
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel AppContext switch + `AddMeter("Microsoft.SemanticKernel.*")` + `AddSource("Microsoft.SemanticKernel.*")`; added `AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true)`.
- **VectorEntities.csproj**: TFM net9 → net10; `Microsoft.Extensions.VectorData.Abstractions` 9.5.0 → 10.6.0.
- **DataEntities, SearchEntities, Store**: TFM net9 → net10 only.

### SK Reference Count

- **3 remaining** (package reference + comment + using directive), all scoped to `Microsoft.SemanticKernel.Connectors.AzureAISearch`.
- Zero SK chat/kernel/agent references.

### ProductVector key type note

Scenario 02's `ProductVector.Id` is `string` (not `int` like scenario 01) because Azure AI Search requires string keys. `GenerateVectorAsync` returns `ReadOnlyMemory<float>` which directly assigned to `ProductVector.Vector` (`ReadOnlyMemory<float>`) — no `.ToArray()` needed.

### Build & Tests

- `dotnet build` → **succeeded** (32 warnings, 0 errors; all warnings are OTel vulnerability advisories matching scenario-01 baseline)
- No test projects in this scenario's solution


## 2026-06-06 — Scenario 11-GitHubModels Modernization

### What Changed

- All projects: net9.0 → **net10.0**
- **eShopAppHost.csproj**: `Aspire.AppHost.Sdk` 9.4.0 → 13.0.1; all `Aspire.Hosting.*` 9.4.0 → 13.0.1; **removed `Aspire.Hosting.Azure.CognitiveServices`** (no longer auto-provisioning Azure OpenAI — replaced by explicit parameters)
- **eShopAppHost/Program.cs**: removed `using Azure.Provisioning.CognitiveServices;`; in **publish mode** replaced `AddAzureOpenAI("openai")` + `AddDeployment(...)` + `WithReference(aoai)` with 4 explicit parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey` secret, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`) + `WithEnvironment("AI_UseGitHubModels", "false")`; in **local mode** replaced single `githubToken` parameter with 4 explicit parameters (`GitHubModelsToken` secret, `GitHubModelsEndpoint` with default, `GitHubModelsChatModel` with default, `GitHubModelsEmbeddingsModel` with default) + `WithEnvironment("AI_UseGitHubModels", "true")`
- **Products.csproj**: removed `Aspire.Azure.AI.OpenAI`, `Microsoft.SemanticKernel.Connectors.InMemory` 1.61.0-preview, `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0, `Microsoft.Extensions.AI` 9.7.0, `Microsoft.Extensions.AI.OpenAI` 9.7.0-preview; added `Azure.AI.OpenAI` 2.1.0, `Azure.Identity` 1.21.0, `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3, `Microsoft.Extensions.AI` 10.6.0, `Microsoft.Extensions.AI.Abstractions` 10.6.0, `Microsoft.Extensions.AI.OpenAI` 10.6.0; bumped `Aspire.Microsoft.EntityFrameworkCore.SqlServer` 9.4.0 → 13.0.1; OTel 1.12.0 → 1.14.0
- **Products/Program.cs**: replaced `OpenAI.Chat.ChatClient` + `OpenAI.Embeddings.EmbeddingClient` registrations with MEAI `IChatClient` + `IEmbeddingGenerator`; GitHub Models path uses `new OpenAIClient(new ApiKeyCredential(token), new OpenAIClientOptions { Endpoint = uri })` + `AsIChatClient()` / `AsIEmbeddingGenerator()`; Azure OpenAI path uses `AzureOpenAIClient` constructed from explicit params + `AsIChatClient()` / `AsIEmbeddingGenerator()`; MemoryContext injection updated to MEAI types
- **Products/Memory/MemoryContext.cs**: `using Microsoft.SemanticKernel.Connectors.InMemory` → `using CommunityToolkit.VectorData.InMemory`; removed `using Microsoft.AspNetCore.Mvc.Formatters`, `using OpenAI.Chat`, `using OpenAI.Embeddings`, `using Newtonsoft.Json`, `using static Microsoft.EntityFrameworkCore.DbLoggerCategory`; `ChatClient?` → `IChatClient?`; `EmbeddingClient?` → `IEmbeddingGenerator<string, Embedding<float>>?`; `_productsCollection` changed from non-nullable to nullable; `GenerateEmbeddingAsync(…).Value.ToFloats()` → `GenerateVectorAsync(…).ToArray()`; `CompleteChatAsync(…).Value.Content[0].Text` → `GetResponseAsync(…).Text`; `new OpenAI.Chat.SystemChatMessage/UserChatMessage` → `new ChatMessage(ChatRole.System/User, …)`; `JsonConvert.SerializeObject` → `JsonSerializer.Serialize`
- **VectorEntities.csproj**: TFM net9 → net10; `Microsoft.Extensions.VectorData.Abstractions` 9.7.0 → 10.6.0
- **eShopServiceDefaults.csproj**: TFM net9 → net10; all packages bumped (OTel 1.12.0 → 1.14.0, Azure.Monitor 1.3.0 → 1.4.0, Http.Resilience 9.7.0 → 10.0.0, ServiceDiscovery 9.4.0 → 10.0.0)
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel switch + `AddMeter("Microsoft.SemanticKernel.*")` + `AddSource("Microsoft.SemanticKernel.*")`; added `AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true)`
- **DataEntities, SearchEntities, CartEntities, Store**: TFM net9 → net10; Store OTel 1.12.0 → 1.14.0
- **Products.Tests.csproj**: TFM net9 → net10; `Microsoft.EntityFrameworkCore.InMemory` 9.0.8 → **10.0.8** (required for EF Core 10.x compatibility)
- **Store.Tests.csproj**: TFM net9 → net10

### GitHub Models Local Path Parameterization

This scenario is **unique** in the portfolio: it has a dual-mode local-vs-deployed switch (`AI_UseGitHubModels` bool env var). The modernized pattern uses explicit Aspire parameters for **both** modes:

**Local mode (GitHub Models):**
```
Parameters:GitHubModelsToken           – GitHub PAT with Models scope (required)
Parameters:GitHubModelsEndpoint        – defaults to https://models.inference.ai.azure.com
Parameters:GitHubModelsChatModel       – defaults to gpt-4.1-mini
Parameters:GitHubModelsEmbeddingsModel – defaults to text-embedding-3-small
```

**Deployed mode (Azure OpenAI):**
```
Parameters:AzureOpenAIEndpoint                 – https://<resource>.openai.azure.com/
Parameters:AzureOpenAIApiKey                   – API key (secret)
Parameters:AzureOpenAIDeploymentName           – e.g. gpt-4.1-mini
Parameters:AzureOpenAIEmbeddingsDeploymentName – e.g. text-embedding-ada-002
```

The parameters for each mode are declared only in their respective `if/else` branch in AppHost, so local developers only need to set GitHub Models params and deployed environments only need AOAI params.

### GitHub Models MEAI Client Construction

```csharp
// GitHub Models uses the OpenAI-compatible endpoint with a GitHub PAT
var client = new OpenAIClient(new ApiKeyCredential(githubToken), new OpenAIClientOptions
{
    Endpoint = new Uri(githubEndpoint) // https://models.inference.ai.azure.com
});
builder.Services.AddChatClient(client.GetChatClient(chatModel).AsIChatClient());
builder.Services.AddEmbeddingGenerator(client.GetEmbeddingClient(embedModel).AsIEmbeddingGenerator());
```

### Key Package Fix

`Microsoft.Extensions.AI` 10.6.0 must be referenced **explicitly** in Products.csproj. The `AddChatClient(IChatClient)` / `AddEmbeddingGenerator(IEmbeddingGenerator)` extension methods on `IServiceCollection` are defined there. `Microsoft.Extensions.AI.OpenAI` 10.6.0 alone only brings `OpenAIHostBuilderExtensions.AddChatClient(IHostApplicationBuilder, string)` — an Aspire-style registration that doesn't match the manual client construction pattern used in this scenario.

### Products.Tests EF Core Version Fix

`Microsoft.EntityFrameworkCore.InMemory` must match the EF Core version brought in by `Aspire.Microsoft.EntityFrameworkCore.SqlServer` (which bumped to 13.0.1 → EF Core 10.x). Using 9.0.8 caused `MissingMethodException` at test init. Fix: bump to **10.0.8**.

### Build & Tests

- `dotnet build eShopLite-GitHubModels.slnx` → **succeeded** (0 errors; NU1902/NU1510 warnings only)
- `dotnet test eShopLite-GitHubModels.slnx` → **7/7 passed** (Products.Tests: 6, Store.Tests: 1)
- `grep Microsoft.SemanticKernel` → **0 results**


## 2026-06-06 — Scenario 03-RealtimeAudio Modernization

### What Changed

- All projects: net9.0 → **net10.0**
- **eShopAppHost.csproj**: Aspire.AppHost.Sdk 9.0.0 → 13.0.1; all Aspire.Hosting.* packages → 13.0.1
- **Products.csproj**: removed Microsoft.SemanticKernel.Connectors.InMemory 1.43.0-preview, Microsoft.VisualStudio.Web.CodeGeneration.Design 9.0.0; bumped Aspire.Azure.AI.OpenAI → 13.0.1-preview.1.25575.3, Aspire.Microsoft.EntityFrameworkCore.SqlServer → 13.0.1; added CommunityToolkit.VectorData.InMemory 1.0.0-preview.3, Microsoft.Extensions.AI.Abstractions 10.6.0, Microsoft.Extensions.AI.OpenAI 10.6.0; OTel → 1.14.0
- **VectorEntities.csproj**: Microsoft.Extensions.VectorData.Abstractions 9.0.0-preview → 10.6.0
- **VectorEntities/ProductVector.cs**: attribute renames [VectorStoreRecordKey]→[VectorStoreKey], [VectorStoreRecordData]→[VectorStoreData], [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]→[VectorStoreVector(384)]
- **eShopServiceDefaults.csproj**: all packages bumped (OTel 1.11.x → 1.14.0, Azure.Monitor → 1.4.0, Http.Resilience → 10.0.0, ServiceDiscovery → 10.0.0)
- **StoreRealtime.csproj**: removed Aspire.Azure.AI.OpenAI (no longer needed — client built directly); kept Azure.AI.OpenAI at **2.1.0-beta.2** (see note below); bumped Microsoft.Extensions.AI 9.1.0-preview → **10.6.0**; added Azure.Identity 1.13.2 explicitly
- **DataEntities, SearchEntities, Store**: TFM net9 → net10 + package bumps

### 5-Parameter Pattern for Realtime Audio

This scenario has **5** Aspire parameters (one extra for the realtime deployment):

| Parameter                              | Secret? | Used By                    |
|----------------------------------------|---------|----------------------------|
| AzureOpenAIEndpoint                  | No      | Products, StoreRealtime    |
| AzureOpenAIApiKey                    | **Yes** | Products, StoreRealtime    |
| AzureOpenAIDeploymentName            | No      | Products                   |
| AzureOpenAIEmbeddingsDeploymentName  | No      | Products                   |
| AzureOpenAIRealtimeDeploymentName    | No      | StoreRealtime              |

User-secrets keys (set in ShopAppHost):
`
Parameters:AzureOpenAIEndpoint                 https://<resource>.openai.azure.com/
Parameters:AzureOpenAIApiKey                   <your-api-key>
Parameters:AzureOpenAIDeploymentName           gpt-4o-mini
Parameters:AzureOpenAIEmbeddingsDeploymentName text-embedding-ada-002
Parameters:AzureOpenAIRealtimeDeploymentName   gpt-4o-mini-realtime-preview
`

### Azure.AI.OpenAI Version Constraint for Realtime

**Azure.AI.OpenAI MUST stay at 2.1.0-beta.2** for the OpenAI.RealtimeConversation namespace to exist.

- In 2.3.0-beta.1 (latest beta as of 2026-06-06), OpenAI.RealtimeConversation was **removed or relocated** — the namespace no longer exists in that version.
- No stable release of Azure.AI.OpenAI exists with 2.x. The latest found is 2.3.0-beta.1.
- The safest choice for preserving audio functionality is to pin 2.1.0-beta.2.
- Azure.Identity must be added **explicitly** to StoreRealtime.csproj because removing Aspire.Azure.AI.OpenAI drops its transitive dep on Azure.Identity.

### Microsoft.Extensions.AI 10.6.0 Breaking Changes (AIFunction)

The AIFunction API changed significantly between 9.x-preview and 10.6.0:

| Old API (9.x)                              | New API (10.6.0)                    |
|--------------------------------------------|-------------------------------------|
| iFunction.Metadata.Name                 | iFunction.Name                   |
| iFunction.Metadata.Description          | iFunction.Description            |
| iFunction.Metadata.Parameters (list)    | iFunction.JsonSchema (JsonElement)|
| iFunction.InvokeAsync(Dictionary<...>)  | iFunction.InvokeAsync(new AIFunctionArguments(dict)) |
| 	.Metadata.Name in LINQ predicate        | 	.Name                            |

JsonSchema returns a System.Text.Json.JsonElement containing the full JSON Schema for the function's parameters (OpenAI tool format). Use .GetRawText() to get the raw JSON for BinaryData.FromString(...).

AIFunctionArguments constructor: 
ew AIFunctionArguments(IDictionary<string, object?> arguments) — pass the deserialized dictionary directly.

### StoreRealtime Program.cs

Replaced the old AddAzureOpenAIClient + regex-based connection-string parsing with direct AzureOpenAIClient construction:
`csharp
AzureOpenAIClient? aoaiClient = string.IsNullOrEmpty(apiKey)
    ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

builder.Services.AddSingleton(serviceProvider =>
{
    RealtimeConversationClient? client = aoaiClient?.GetRealtimeConversationClient(realtimeDeploymentName);
    return client!;
});
`

### Build & Tests

- dotnet build eShopLite-RealtimeAudio.slnx → **Build succeeded** (0 errors; NU1902 vulnerability warnings only — same OTel advisories as other scenarios)
- dotnet test eShopLite-RealtimeAudio.slnx → **No test projects** (Build succeeded)
- grep Microsoft.SemanticKernel → **0 results**


## 2026-06-06 -- Scenario 06-mcp Modernization (sc-06 + sk-06-mcp-agents fulfilled)

### What Changed

- All projects: net9.0 -> net10.0
- eShopAppHost.csproj: Aspire.AppHost.Sdk 9.1.0 -> 13.0.1; all Aspire.Hosting.* 9.3.1 -> 13.0.1
- eShopServiceDefaults.csproj: OTel 1.12.0 -> 1.14.0; Azure.Monitor 1.3.0 -> 1.4.0; Http.Resilience 9.6.0 -> 10.0.0; ServiceDiscovery 9.3.1 -> 10.0.0
- eShopServiceDefaults/Extensions.cs: removed SK OTel switch (Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive), AddMeter/AddSource SK wildcards; added Azure.Experimental.EnableActivitySource
- VectorEntities.csproj: Microsoft.Extensions.VectorData.Abstractions 9.6.0 -> 10.6.0
- Products.csproj: removed Microsoft.SemanticKernel.Connectors.InMemory 1.57.0-preview, Microsoft.VisualStudio.Web.CodeGeneration.Design 9.0.0, System.Text.Json 9.0.6; bumped Aspire.Azure.AI.OpenAI -> 13.0.1-preview.1.25575.3; Aspire.Microsoft.EntityFrameworkCore.SqlServer -> 13.0.1; added CommunityToolkit.VectorData.InMemory 1.0.0-preview.3, Microsoft.Extensions.AI.Abstractions 10.6.0, Microsoft.Extensions.AI.OpenAI 10.6.0
- Products/Memory/MemoryContext.cs: SK InMemory -> CommunityToolkit InMemory; ChatClient -> IChatClient; EmbeddingClient -> IEmbeddingGenerator<string,Embedding<float>>; GenerateEmbeddingAsync/ToFloats -> GenerateVectorAsync/ToArray; CompleteChatAsync -> GetResponseAsync; SystemChatMessage/UserChatMessage -> ChatMessage(ChatRole...); Newtonsoft.Json -> System.Text.Json
- Products/Program.cs: removed builder.AddAzureOpenAIClient; added 4-param explicit AzureOpenAIClient + AsIChatClient / AsIEmbeddingGenerator; updated MemoryContext DI registration
- Products/Endpoints/ProductEndpoints.cs: removed unused OpenAI.Embeddings and OpenAI.Chat usings
- OnlineResearcher.csproj: removed ALL Microsoft.SemanticKernel.* packages (4: Agents.Abstractions, Agents.Core, Agents.OpenAI, Connectors.AzureOpenAI); updated Microsoft.AspNetCore.OpenApi 9.0.6 -> 10.0.0; removed redundant UserSecrets
- eShopMcpSseServer.csproj: bumped Aspire.Azure.AI.OpenAI -> 13.0.1-preview.1.25575.3; added Microsoft.Extensions.AI.OpenAI 10.6.0; removed System.Text.Json 9.0.6
- eShopMcpSseServer/Program.cs: replaced builder.AddAzureOpenAIClient + ChatClient with 4-param explicit AOAI + AddChatClient(AsIChatClient())
- eShopMcpSseServer/Tools/OnlineResearch.cs: ChatClient -> IChatClient; CompleteChatAsync -> GetResponseAsync
- Store.csproj: bumped Aspire.Azure.AI.OpenAI -> 13.0.1-preview.1.25575.3; Microsoft.Extensions.AI.OpenAI 9.6.0-preview -> 10.6.0; OTel 1.12.0 -> 1.14.0
- Store/Program.cs: replaced AddOpenAIClient/AddAzureOpenAIClient + dual registrations with 4-param explicit AOAI + AddChatClient with UseFunctionInvocation
- eShopAppHost/Program.cs: 4 Aspire params (AzureOpenAIEndpoint, AzureOpenAIApiKey secret, AzureOpenAIDeploymentName, AzureOpenAIEmbeddingsDeploymentName); wired all 4 to products; first 3 to eshopmcpserver and store; IsPublishMode branch kept
- WeatherAgent, ParkInformationAgent: net10.0; Microsoft.AspNetCore.OpenApi 9.0.6 -> 10.0.0
- Services.csproj: net10.0; Microsoft.Extensions.Logging.Abstractions 9.0.6 -> 10.0.0
- DataEntities, SearchEntities, McpToolsEntities: net10.0 only

### SK Migration Finding

The OnlineResearcher csproj listed 4 Microsoft.SemanticKernel.* packages but the actual source code did NOT use any SK APIs. It had already been partially migrated to use Azure.AI.Agents.Persistent (PersistentAgentsClient with BingGroundingToolDefinition). The SK packages were orphaned dead references. Action: simply removed them. No source changes needed for agent logic.

### MCP + MEAI Integration Pattern

MCP tool methods receive services via DI injection by parameter type. When switching from ChatClient to IChatClient, only the parameter type in the tool method signature changes -- ModelContextProtocol.Server resolves parameters from the ASP.NET DI container automatically. No extra wiring needed.

### MAF Usage

No Microsoft.Agents.AI was added because no ChatCompletionAgent/AddKernel code existed in the actual source. The scenario's agent behavior uses: (1) Azure.AI.Agents.Persistent PersistentAgentsClient in OnlineResearcher for Bing grounding, (2) MEAI IChatClient with MCP tool invocation in eShopMcpSseServer/Store.

### Build / Test

- dotnet build eShopLite-Aspire-mcp.slnx -> Build succeeded (0 errors; NU1902 OTel vulnerability warnings only)
- dotnet test eShopLite-Aspire-mcp.slnx -> No test projects (exit 0)
- grep Microsoft.SemanticKernel -> 0 results

## 2026-06-06 -- Scenario 07-AgentsConcurrent Modernization (sk-07-agents fulfilled)

### SK to MAF: Insights Agent Migration

All 5 SK packages removed from Insights.csproj (Microsoft.SemanticKernel, Agents.Core, Agents.Orchestration, Agents.Runtime.InProcess, Connectors.AzureOpenAI). Replaced with:
- Microsoft.Agents.AI 1.0.0-preview.251125.1
- Microsoft.Agents.AI.Abstractions 1.0.0-preview.251125.1
- Microsoft.Agents.AI.OpenAI 1.0.0-preview.251125.1
- Microsoft.Agents.AI.Workflows 1.0.0-preview.251125.1
- Microsoft.Extensions.AI 10.0.1 + Abstractions 10.6.0 + OpenAI 10.6.0
- Azure.Identity 1.17.0 (Aspire.Azure.AI.OpenAI 13.0.1-preview requires >=1.17.0)

### MAF Concurrent Orchestration Pattern

Original SK used ConcurrentOrchestration + InProcessRuntime. MAF replacement uses
AgentWorkflowBuilder.BuildSequential per agent and Task.WhenAll for concurrency:

  var sentimentWorkflow = AgentWorkflowBuilder.BuildSequential('SentimentWorkflow', [sentimentAgent]);
  var languageWorkflow  = AgentWorkflowBuilder.BuildSequential('LanguageWorkflow', [languageAgent]);
  var st = InProcessExecution.RunAsync(sentimentWorkflow, sentimentMessages).AsTask();
  var lt = InProcessExecution.RunAsync(languageWorkflow,  languageMessages).AsTask();
  await Task.WhenAll(st, lt);
  var sentimentText = st.Result.OutgoingEvents.OfType<AgentRunResponseEvent>().LastOrDefault()?.Response.Text;
  var languageText  = lt.Result.OutgoingEvents.OfType<AgentRunResponseEvent>().LastOrDefault()?.Response.Text;

Key MAF 1.0.0-preview.251125.1 API facts:
- InProcessExecution.RunAsync returns ValueTask<Run> -- call .AsTask() before Task.WhenAll
- Run.OutgoingEvents.OfType<AgentRunResponseEvent>() -- events from workflow
- AgentRunResponseEvent.Response.Text -- agent response text
- AgentWorkflowBuilder.BuildSequential(name, agents[]) -- linear workflow, safe per-request
- chatClient.CreateAIAgent(name, instructions) -- extension from Microsoft.Agents.AI
- services.AddKeyedSingleton<AIAgent>(name, factory) -- DI pattern for agents

### Package Gotcha

Aspire.Azure.AI.OpenAI 13.0.1-preview.1.25575.3 requires Azure.Identity >= 1.17.0.
Pinning to 1.13.2 causes NU1605 downgrade error. Always use Azure.Identity >= 1.17.0
when this Aspire preview package is referenced.

### Build / Test

- dotnet build eShopLite-Agents-Concurrent.slnx: Build succeeded (0 errors)
- dotnet test: No test projects (exit 0)
- grep Microsoft.SemanticKernel src/: 0 results


## 2026-06-06 -- Scenario 04-chromadb Modernization

### Chroma Vector Provider Decision

No SK-named Chroma VectorData connector exists on NuGet as of 2026-06-06. Scenario-04 did NOT use an SK connector at all -- it used ChromaDB.Client 1.0.1-ci-13369893450 (community package) directly for all Chroma vector operations (Upsert, Query). The only SK package was Microsoft.SemanticKernel.Connectors.InMemory which was referenced in Products.csproj but NEVER used in any source code (dead import). Removing it was sufficient.

ChromaDB.Client usage pattern (non-SK):
- ChromaClient creates/gets a collection
- ChromaCollectionClient.Upsert(ids, embeddings, metadatas) -- takes IEnumerable<ReadOnlyMemory<float>> for embeddings
- ChromaCollectionClient.Query(queryEmbeddings, nResults, include) -- takes ReadOnlyMemory<float> for the query vector
- ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances for result detail

### CRITICAL: VectorData 10.6.0 Attribute Rename

Microsoft.Extensions.VectorData.Abstractions 10.6.0 renamed the VectorStore record attributes:

| Old (9.x)                                         | New (10.6.0)            |
|---------------------------------------------------|-------------------------|
| [VectorStoreRecordKey]                            | [VectorStoreKey]        |
| [VectorStoreRecordData]                           | [VectorStoreData]       |
| [VectorStoreRecordVector(size, DistanceFunction)] | [VectorStoreVector(size)]|

Failing to update these produces CS0246 type-not-found errors. The net10.0 DLL in the package has 0 exported types in PowerShell reflection (MEAI Abstractions dependency makes it appear empty) -- misleading but compiler resolves fine when all references are provided.

### What Changed

- All projects: net9.0 -> net10.0; Aspire 9.x -> 13.0.1; packages updated
- eShopAppHost/Program.cs: removed openai connection string + old env vars; added 4 explicit parameters (AzureOpenAIEndpoint, AzureOpenAIApiKey secret, AzureOpenAIDeploymentName, AzureOpenAIEmbeddingsDeploymentName); renamed endpoint -> chromaEndpoint; kept Chroma container wiring
- Products.csproj: removed SK.Connectors.InMemory 1.47.0-preview + CodeGeneration.Design; bumped Aspire.Azure.AI.OpenAI -> 13.0.1-preview.1.25575.3; added MEAI Abstractions + MEAI.OpenAI 10.6.0; kept ChromaDB.Client
- Products/Program.cs: replaced AddAzureOpenAIClient + ChatClient/EmbeddingClient with MEAI IChatClient + IEmbeddingGenerator; kept ChromaCollectionClient singleton
- Products/Memory/MemoryContext.cs: ChatClient -> IChatClient; EmbeddingClient -> IEmbeddingGenerator<string, Embedding<float>>; GenerateEmbeddingAsync -> GenerateVectorAsync; CompleteChatAsync -> GetResponseAsync; Newtonsoft.Json -> System.Text.Json; removed unused VectorSearchOptions
- VectorEntities/ProductVector.cs: attribute renames (see table above)
- eShopServiceDefaults: OTel 1.11.x -> 1.14.0; Azure.Monitor 1.3.0-beta.3 -> 1.4.0

### Build & Tests

- dotnet build eShopLite-ChromaDB.slnx -> Build succeeded (0 errors; NU1902 warnings only)
- dotnet test -> No test projects in this solution
- grep Microsoft.SemanticKernel -> 0 results
- Docker/Chroma container: N/A (no test projects)


## 2026-06-06 — Scenario 12-AzureFunctions Modernization

### What Changed

- All projects: net8.0/net9.0 → **net10.0**
- **eShopAppHost.csproj**: Aspire.AppHost.Sdk 9.x → 13.0.1; all Aspire.Hosting.* → 13.0.1; Aspire.Hosting.Azure.Functions → 13.0.1-preview.1.25575.3 (preview-only at 13.0.x level; stable 13.4.x exists)
- **eShopServiceDefaults.csproj**: all OTel → 1.14.0; Azure.Monitor.OpenTelemetry.AspNetCore → 1.4.0; Microsoft.Extensions.* → 10.0.0
- **SemanticSearchFunction.csproj**: net8.0 → net10.0; Microsoft.Azure.Functions.Worker 1.20.0 → **2.52.0** (ships lib/net10.0/ — confirmed net10 support); Microsoft.Azure.Functions.Worker.Sdk 1.16.x → **2.0.7**; Microsoft.Azure.Functions.Worker.Extensions.Http → 3.3.0; Microsoft.Azure.Functions.Worker.ApplicationInsights → 2.51.0; removed Aspire.Azure.AI.OpenAI + Aspire.Azure.AI.Inference; added Azure.AI.OpenAI 2.1.0, Azure.Identity 1.21.0, Microsoft.Extensions.AI.Abstractions 10.6.0, Microsoft.Extensions.AI.OpenAI 10.6.0
- **eShopAppHost/Program.cs**: replaced AddConnectionString("openai") + AI_ChatDeploymentName env vars with 4 AddParameter() calls; both Products and SemanticSearchFunction receive all 4 via WithEnvironment
- **SemanticSearchFunction/Program.cs**: complete rewrite — reads 4 env vars, constructs AzureOpenAIClient directly, registers IEmbeddingGenerator, registers ISemanticSearchRepository DI
- **SemanticSearchFunction/Repositories/ISemanticSearchRepository.cs**: **NEW** — interface enabling Moq-based unit testing of SearchFunction
- **SemanticSearchFunction/Functions/SearchFunction.cs**: dependency changed from SqlSemanticSearchRepository (concrete) → ISemanticSearchRepository (interface)
- **eShopServiceDefaults/Extensions.cs**: removed SK OTel switch and meter/source wildcards (Microsoft.SemanticKernel.*) — same cleanup as all other scenarios

### Azure Functions Worker net10 — Key Findings

- **Worker 2.52.0** is the version that ships a lib/net10.0/ folder — confirmed via NuGet manifest
- **CreateResponse(HttpStatusCode) is now an extension method** in HttpRequestDataExtensions (NOT abstract on HttpRequestData). Moq cannot mock extension methods → in tests, mock CreateResponse() (no-arg abstract) instead, then test the response object separately
- HttpResponseData.Body is abstract with get/set; use SetupGet(r => r.Body).Returns(new MemoryStream()) in Moq setup
- **Aspire.Hosting.Azure.Functions**: stays at 13.0.1-preview.1.25575.3 (no stable 13.0.x release; stable baseline jumps to 13.4.x)

### EF Core Version Alignment (VectorSearch constraint)

- EFCore.SqlServer.VectorSearch 9.0.0 pins EF Core to **9.x** (uses internal 9.x Relational assembly APIs)
- Test projects must use Microsoft.EntityFrameworkCore.InMemory **9.0.16** (latest 9.x patch)
- Using InMemory 10.0.0 with EF Core 9.x production code causes MissingMethodException at runtime (same root cause as SC-08)

### MSTest 4.x Breaking Change (sync assertions)

- Assert.ThrowsException<T>() was **removed** — replace with Assert.ThrowsExactly<T>() for synchronous code
- Assert.ThrowsExactlyAsync<T>() for async equivalents

### ISemanticSearchRepository Pattern

Introducing an interface for the Azure Functions repository:
- Enables Mock<ISemanticSearchRepository> in SearchFunctionTests without needing a real DB
- SearchFunction constructor takes ISemanticSearchRepository (not the concrete class)
- DI registration in Program.cs: services.AddScoped<ISemanticSearchRepository, SqlSemanticSearchRepository>()

### Package Versions Used

| Package | Version |
|---------|---------|
| Microsoft.Azure.Functions.Worker | **2.52.0** |
| Microsoft.Azure.Functions.Worker.Sdk | **2.0.7** |
| Microsoft.Azure.Functions.Worker.Extensions.Http | 3.3.0 |
| Microsoft.Azure.Functions.Worker.ApplicationInsights | 2.51.0 |
| Aspire.Hosting.Azure.Functions | 13.0.1-preview.1.25575.3 |
| Azure.AI.OpenAI | 2.1.0 |
| Azure.Identity | 1.21.0 |
| Microsoft.Extensions.AI.Abstractions | 10.6.0 |
| Microsoft.Extensions.AI.OpenAI | 10.6.0 |
| Microsoft.EntityFrameworkCore.InMemory (tests) | 9.0.16 |
| MSTest | 4.2.3 |
| Moq | 4.20.72 |

### Build / Test

- dotnet build eShopLite-AzFnc.slnx → **Build succeeded** (0 errors)
- dotnet test eShopLite-AzFnc.slnx → **13/13 passed** (SemanticSearch.Tests: 6, Products.Tests: 7, 0 skipped)
- grep Microsoft.SemanticKernel → **0 results**
