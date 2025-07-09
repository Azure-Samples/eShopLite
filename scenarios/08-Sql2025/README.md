[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)

## Description

**eShopLite - Semantic Search using SQL 2025** is a reference .NET application implementing an eCommerce site with Search features using vector search and vector indexes in the SQL Database Engine

- [Features](#features)
- [Architecture diagram](#architecture-diagram)
- [Getting started](#getting-started)
- [Deploying to Azure](#deploying)
- Run solution
- [Resources](#resources)
- [Video Recordings](#video-recordings)
- [Guidance](#guidance)
  - [Costs](#costs)
  - [Security Guidelines](#security-guidelines)
- [Resources](#resources)

## Features

**GitHub CodeSpaces:** This project is designed to be opened in GitHub Codespaces as an easy way for anyone to deploy the solution entirely in the browser.

This is the eShopLite Application running, performing a **Keyword Search**:

![eShopLite Application running doing search using keyword search](./images/05eShopLite-SearchKeyWord.gif)

This is the eShopLite Application running, performing a **Semantic Search**:

![eShopLite Application running doing search using keyword search](./images/06eShopLite-SearchSemantic.gif)

## Architecture diagram

  ```mermaid
  flowchart TD
      subgraph "Azure Container Apps Environment - .NET Aspire"
          store[store service]
          products[products service]
          sql[SQL service]
          store --> products
          products --> sql
      end
  
      ContainerRegistry[Container Registry]
      StorageAccount[Storage Account]
      ManagedIdentity[Managed Identity]
      OpenAI[Azure OpenAI\nChat + Embeddings]
      AISearch[Azure AI Search\nVector Index]
      LogAnalytics[Log Analytics]
  
      ContainerRegistry --> ManagedIdentity
      ManagedIdentity --> OpenAI
      ManagedIdentity --> AISearch
  
      products --> ManagedIdentity
      products -->|semantic search| AISearch
      products -->|generate embeddings + chat| OpenAI
  
      store --> LogAnalytics
      products --> LogAnalytics
  
  ```

## Getting Started

## Main Concepts in this Scenario

This scenario demonstrates how to use [SQL Server 2025's Vector search and Vector index features](https://learn.microsoft.com/en-us/sql/relational-databases/vectors/vectors-sql-server?view=sql-server-ver17) in a .NET Aspire application. The main concepts and implementation details are:

- The .NET Aspire AppHost project creates the SQL Server 2025 instance directly using the container image `mcr.microsoft.com/mssql/server:2025-latest` from the Docker repository for SQL Server 2025: [Microsoft SQL Server - Ubuntu based images](https://hub.docker.com/r/microsoft/mssql-server/).

- The logic for initializing and running the SQL Server container is implemented in [`scenarios/08-Sql2025/src/eShopAppHost/Program.cs`](scenarios/08-Sql2025/src/eShopAppHost/Program.cs):

    ```csharp
    var builder = DistributedApplication.CreateBuilder(args);
    
    var sql = builder.AddSqlServer("sql")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithImageTag("2025-latest")
        .WithEnvironment("ACCEPT_EULA", "Y");
    
    var productsDb = sql
        .WithDataVolume()
        .AddDatabase("productsDb");
    
    var products = builder.AddProject<Projects.Products>("products")
        .WithReference(productsDb)
        .WaitFor(productsDb);    
    ```

- Using an embedding client, once the database is initialized and a set of sample products is added, a new vector field is completed using an embedding. This logic is in [`scenarios/08-Sql2025/src/Products/Models/DbInitializer.cs`](./src/Products/Models/DbInitializer.cs).

- The `ProductApiActions` class ([`scenarios/08-Sql2025/src/Products/Endpoints/ProductApiActions.cs`](./src/Products/Endpoints/ProductApiActions.cs)) implements an `AISearch()` function that performs semantic search using [EFCore.SqlServer.VectorSearch](https://www.nuget.org/packages/EFCore.SqlServer.VectorSearch/9.0.0-preview.2#show-readme-container) functions:

    ```csharp
    public static async Task<IResult> AISearch(string search, Context db, EmbeddingClient embeddingClient, int dimensions = 1536)
    {
        Console.WriteLine("Querying for similar products...");
    
        var embeddingSearch = embeddingClient.GenerateEmbedding(search, new() { Dimensions = dimensions });
        var vectorSearch = embeddingSearch.Value.ToFloats().ToArray();
        var products = await db.Product
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
            .Take(3)
            .ToListAsync();
    
        var response = new SearchResponse
        {
            Products = products,
            Response = products.Count > 0 ?
                $"{products.Count} Products found for [{search}]" :
                $"No products found for [{search}]"
        };
        return Results.Ok(response);
    }
    ```

These components work together to enable semantic search over product data using SQL Server 2025's vector capabilities.

The solution is in the `./src` folder, the main solution is **[eShopLite-Sql2025.slnx](./src/eShopLite-Sql2025.slnx)**.

## Deploying

> **Note:** The deployment process for this scenario is the same as in [scenario 01 - Semantic Search](../01-SemanticSearch/README.md). Please refer to the [Deploying](../01-SemanticSearch/README.md#deploying) section in that README for detailed steps and requirements. All Azure resource provisioning, local development, and Codespaces instructions are identical for this scenario.

## Guidance

### Costs

For **Azure OpenAI Services**, pricing varies per region and usage, so it isn't possible to predict exact costs for your usage.
The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers.
However, Azure Container Registry has a fixed cost per registry per day.

You can try the [Azure pricing calculator](https://azure.com/e/2176802ea14941e4959eae8ad335aeb5) for the resources:

- Azure OpenAI Service: S0 tier, gpt-4.1-mini and text-embedding-3-small models. Pricing is based on token count. [Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
- Azure Container App: Consumption tier with 0.5 CPU, 1GiB memory/storage. Pricing is based on resource allocation, and each month allows for a certain amount of free usage. [Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- Azure Container Registry: Basic tier. [Pricing](https://azure.microsoft.com/pricing/details/container-registry/)
- Log analytics: Pay-as-you-go tier. Costs based on data ingested. [Pricing](https://azure.microsoft.com/pricing/details/monitor/)
- Azure Application Insights pricing is based on a Pay-As-You-Go model. [Pricing](https://learn.microsoft.com/azure/azure-monitor/logs/cost-logs).

⚠️ To avoid unnecessary costs, remember to take down your app if it's no longer in use, either by deleting the resource group in the Portal or running `azd down`.

### Security Guidelines

Samples in this templates uses Azure OpenAI Services with ApiKey and [Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

The Main Sample uses Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

Additionally, we have added a [GitHub Action](https://github.com/microsoft/security-devops-action) that scans the infrastructure-as-code files and generates a report containing any detected issues. To ensure continued best practices in your own repository, we recommend that anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/code-security/secret-scanning/about-secret-scanning) setting is enabled.

You may want to consider additional security measures, such as:

- Protecting the Azure Container Apps instance with a [firewall](https://learn.microsoft.com/azure/container-apps/waf-app-gateway) and/or [Virtual Network](https://learn.microsoft.com/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

## Resources

- [Deploy a .NET Aspire project to Azure Container Apps using the Azure Developer CLI (in-depth guide)](https://learn.microsoft.com/dotnet/aspire/deployment/azure/aca-deployment-azd-in-depth)

- [Aspiring .NET Applications with Azure OpenAI](https://learn.microsoft.com/shows/azure-developers-dotnet-aspire-day-2024/aspiring-dotnet-applications-with-azure-openai)

### Video Recordings

[![Run eShopLite Semantic Search in Minutes with .NET Aspire & GitHub Codespaces 🚀](./images/90ytrunfromcodespaces.png)](https://youtu.be/T9HwjVIDPAE)

## Want to know more?

For detailed technical documentation, SQL Server 2025 implementation guides, and modern database architecture patterns, visit our comprehensive documentation:

📚 **[Detailed Documentation](./docs/README.md)** - Complete technical documentation for this scenario

### What you'll find in the docs:
- 🗄️ **SQL Server 2025 Deep Dive** - Latest features, container optimization, and performance
- 🔍 **Vector Search Integration** - Native SQL vector capabilities and EF Core patterns
- 🏗️ **Database Architecture** - Modern Entity Framework patterns and optimization
- 📊 **Performance Tuning** - Query optimization and connection management
- 🚀 **Container Deployment** - Docker/Podman integration and lifecycle management
- ⚙️ **Configuration Patterns** - Development vs production setup strategies
- 🔄 **Migration Strategies** - Upgrading from previous SQL Server versions

Perfect for developers modernizing database applications or exploring SQL Server 2025's cutting-edge capabilities.
