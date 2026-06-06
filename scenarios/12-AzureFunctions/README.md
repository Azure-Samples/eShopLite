![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)

# eShopLite – Store + Products + Semantic Search (.NET Aspire)

This repository contains a simplified e‑commerce sample (Store frontend + Products backend) extended with Semantic Search capabilities. The solution demonstrates:

* Orchestration and service discovery using **.NET Aspire AppHost**
* Three different product search experiences in the Store UI
* Re–use of SQL Server 2025 vector search capabilities (single mention). For detailed guidance about SQL Server 2025 vector features, indexing, and setup, see the Scenario 08 documentation: `../08-Sql2025/README.md`.
* Optional Azure Functions façade for semantic search

> NOTE: Detailed inner workings of SQL Server 2025 vector indexes and setup are out of scope in this README. For comprehensive guidance about SQL Server 2025 vector capabilities and setup, please refer to Scenario 08: `../08-Sql2025/README.md`.

## Table of contents

* [Search Features](#search-features)
* [Architecture](#architecture)
  * [High level (Mermaid)](#high-level-mermaid)
  * [Search flows](#search-flows)
* [Projects](#projects)
* [Configuration](#configuration)
* [Running locally](#running-locally)
* [Search modes in detail](#search-modes-in-detail)
* [Scenario 01 Guidance & Costs](#scenario-01-guidance--costs)
* [Troubleshooting](#troubleshooting)
* [Contributing](#contributing)
* [License](#license)

## Search Features

The Store UI exposes 3 search options (dropdown / selector on the Search page):

1. Keyword Search (standard text filtering)  
    *Implementation:* `Products` API endpoint in `src/Products/Endpoints/ProductApiActions.cs` (e.g. simple `WHERE` / LINQ filtering).
2. Semantic Search (direct API)  
    *Implementation:* Products API builds an embedding, performs SQL Server 2025 vector similarity query; defined in `ProductApiActions.cs`.
3. Semantic Search via Azure Function  
    *Implementation:* Store calls `SemanticSearchFunction` (`src/SemanticSearchFunction/Functions/SearchFunction.cs`), which performs the same underlying SQL vector similarity, returning top matches.

Animated examples were previously provided as GIFs; if/when assets are added back place them under `./images/` and reference here. For now the flows are described below.

## Architecture

### High level (Mermaid)

```mermaid
flowchart LR
     subgraph AppHost[.NET Aspire AppHost]
          Store[<b>Store Blazor UI</b><br>3 search options]
          Products[Products Service]
          SemanticFn[Semantic Search Azure Function]
          SQL[SQL Server 2025 - Vector Index]
     end

     Store -->|<b>Keyword</b><br>Standard SQL Search| Products
     Store -->|<b>Semantic - WebAPI</b><br>SQL Vector Search| Products
     Store -->|<b>Semantic - Azure Function</b><br>SQL Vector Search| SemanticFn
     Products -->|Keywords and Vector Search| SQL
     SemanticFn -->|Vector Search| SQL

     classDef svc fill:#eef,stroke:#336;
     class Store,Products,SemanticFn svc;
```

### Search flows

```mermaid
sequenceDiagram
     participant User
     participant Store
     participant Products as Products (WebAPI)
     participant SemanticFn as Azure Function
     participant SQL as SQL 2025 (Vectors)

     Note over User,Store: 1) Keyword Search
     User->>Store: Enter term + select Keyword
     Store->>Products: GET /products?search=term
     Products->>SQL: Text filter query
     Products-->>Store: Product list
     Store-->>User: Render results

     Note over User,Store: 2) Semantic (WebAPI)
     User->>Store: Enter query + select Semantic (WebAPI)
     Store->>Products: POST /products/semantic { query }
     Products->>Products: Generate embedding
     Products->>SQL: Vector similarity search
     SQL-->>Products: Top N products
     Products-->>Store: Results
     Store-->>User: Render

     Note over User,Store: 3) Semantic (Azure Function)
     User->>Store: Enter query + select Semantic (Function)
     Store->>SemanticFn: POST /api/semanticsearch { query }
     SemanticFn->>SemanticFn: Generate embedding
     SemanticFn->>SQL: Vector similarity search
     SQL-->>SemanticFn: Top N products
     SemanticFn-->>Store: Results
     Store-->>User: Render
```

## Projects

| Project | Purpose |
| ------- | ------- |
| `src/eShopAppHost` | Aspire AppHost orchestrating services (service discovery, lifecycles). |
| `src/Store` | Front-end (Blazor) calling Products API & Azure Function. |
| `src/Products` | Product catalog API, keyword + direct semantic search (vector). |
| `src/SemanticSearchFunction` | Azure Function exposing semantic search endpoint. |
| `src/DataEntities`, `src/SearchEntities` | Data contract / model assemblies. |
| `src/eShopServiceDefaults` | Shared service discovery + resilience registrations (`AddServiceDefaults`). |
| `tests` projects | Unit tests (`Products.Tests`, `SemanticSearch.Tests`). |

## Running locally

1. Set Azure OpenAI parameters as user secrets in the `eShopAppHost` project. Run these commands from the **repo root**:

```bash
aspire secret set Parameters:AzureOpenAIEndpoint "https://<your-resource>.openai.azure.com/" --apphost scenarios/12-AzureFunctions/src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIApiKey "<your-api-key>" --apphost scenarios/12-AzureFunctions/src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIDeploymentName "gpt-4.1-mini" --apphost scenarios/12-AzureFunctions/src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIEmbeddingsDeploymentName "text-embedding-3-small" --apphost scenarios/12-AzureFunctions/src/eShopAppHost/eShopAppHost.csproj
```

> **Tip:** Run `pwsh .\scripts\Set-AzureOpenAISecrets.ps1` from the repo root to set the 4 common Azure OpenAI values for every scenario at once.

2. Build & start via AppHost:

```powershell
dotnet run --project src/eShopAppHost
```

The dashboard lists services (Store, Products, SemanticSearchFunction, SQL). Open the Store UI, navigate to Search page.

## Search modes in detail

| Mode | Trigger | Code Path | Notes |
| ---- | ------- | --------- | ----- |
| Keyword | Selector: Keyword | `ProductApiActions` (standard endpoint) | Simple filter / LIKE logic. |
| Semantic (WebAPI) | Selector: Semantic (API) | `ProductApiActions` semantic endpoint | Embedding + vector distance query. |
| Semantic (Function) | Selector: Semantic (Azure Function) | `SearchFunction.cs` | Embedding + vector distance query. |

All semantic paths execute a SQL 2025 vector similarity search. For full technical documentation and implementation details, see Scenario 08: `../08-Sql2025/README.md`.

## Scenario 01 Guidance & Costs

Scenario 01 (this repository baseline) guidance, including cost considerations (compute/container sizing, Azure Function consumption vs dedicated, SQL Server licensing / vector feature resource usage) is centralized in:  
[`docs/scenario-01-guidance.md`](./docs/scenario-01-guidance.md)

Summary:

* Azure Function semantic search adds an independent scaling surface (can scale independently from Products service).
