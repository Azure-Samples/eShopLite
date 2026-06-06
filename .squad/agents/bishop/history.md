# Bishop History

## Project Snapshot

## Executive Summary

**Role:** Reference pattern architect and 12-scenario .NET 10 modernization lead

**Completed Work:**
- 01-SemanticSearch: Reference pattern (VectorData InMemory, MEAI, MAF)
- 02-AzureAISearch: Aspire parameter modernization
- 03-RealtimeAudio: Azure.AI.OpenAI realtime API
- 04-chromadb: Chroma DB integration
- 05-deepseek: DeepSeek-R1 reasoning model + keyed registration
- 06-mcp: Model Context Protocol
- 07-AgentsConcurrent: Multi-agent orchestration
- 08-Sql2025: SQL Server 2025 vector search
- 09-AzureAppService: App Service deployment pattern
- 10-A2ANet: Agent-to-Agent protocol
- 11-GitHubModels: GitHub Models integration
- 12-AzureFunctions: Azure Functions façade
- 14-MAFDevUI: MAF Development UI

**Key Outcomes:**
- All scenarios: .NET 10 SDK, Aspire 13, MEAI + CommunityToolkit.VectorData
- Removed Semantic Kernel (except 02 pure vector connector)
- Standardized 4-parameter Aspire shape (endpoint, key, chat deployment, embeddings deployment)
- Approved package exceptions documented

## Previous Session Notes

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

*(Full detailed scenario logs archived in .squad/decisions.md)*
