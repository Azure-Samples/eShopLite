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

## 2026-06-06 — t3-analysis: Azure AI Search VectorStore Connector Document

**Task:** Write explanation + recommendation doc on the single remaining SK package in the repo.

**Deliverable:** `scenarios\02-AzureAISearch\docs\azure-ai-search-vectorstore.md`

**Key findings:**
- `Microsoft.SemanticKernel.Connectors.AzureAISearch` 1.74.0-preview is the sole SK reference repo-wide.
- It is a pure `Microsoft.Extensions.VectorData` connector; no SK kernel/chat/agent runtime is pulled in.
- `Microsoft.Extensions.VectorData.Abstractions` reached GA in May 2025.
- As of 2026-06-06: no non-SK-named Azure AI Search VectorData connector exists on NuGet (`CommunityToolkit.VectorData.AzureAISearch` and `Microsoft.Extensions.VectorData.AzureAISearch` both not found).
- **Recommendation: Option A — keep the SK-named connector.** Lowest effort, fully supported, zero SK runtime. Revisit when a neutral package appears or when the connector leaves `-preview`.

## Learnings
- 2026-06-06T14:05:16.188-04:00: Started the agentic modernization implementation by cloning the closest working scenario baselines (01 for 13-15, 06 for 16, 10 for 17) so each new scenario stays runnable while the story is added.
- 2026-06-06T14:05:16.188-04:00: The new session documentation lives under docs/26 06 16 NET Agentic Modernization/ and centers the narrative on observability, product discovery, store intelligence, MCP tools, and A2A.
- 2026-06-06T14:05:16.188-04:00: The reusable backend pattern is to keep Aspire wiring explicit and preserve the existing 4-parameter Azure OpenAI shape while documenting any non-01 baseline choice in the scenario README.

- 2026-06-07T12:07:27.924-04:00: Standardized the shared semantic-search prompt in all Products/Memory/MemoryContext.cs variants to explicitly request markdown output using concise sections and bullet lists while preserving existing outdoor-domain and comparison/detail intent.
