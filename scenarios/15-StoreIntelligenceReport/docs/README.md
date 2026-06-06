# eShopLite - Semantic Search Documentation

This documentation provides detailed technical information about the eShopLite Semantic Search scenario, including service architecture, AI integrations, and implementation details.

## Overview

The 01-SemanticSearch scenario demonstrates a comprehensive eCommerce platform with advanced search capabilities using both traditional keyword search and AI-powered semantic search. The solution leverages Azure OpenAI services for embeddings and chat functionality, providing users with intelligent product discovery capabilities.

## Features Documentation

- [Aspire Orchestration](./aspire-orchestration.md) - .NET Aspire service composition and configuration
- [Azure OpenAI Integration](./azure-openai-integration.md) - Chat and embedding client setup
- [Semantic Search](./semantic-search.md) - Vector-based product search implementation
- [Memory Context](./memory-context.md) - In-memory vector store for product embeddings
- [SQL Server Database](./sql-server-database.md) - Entity Framework and data modeling
- [Product API Endpoints](./product-api-endpoints.md) - REST API for product operations

## Architecture

```
       ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
       │   Store (UI)    │───▶│  Products API   │───▶│   SQL Server    │
       └─────────────────┘    └─────────────────┘    └─────────────────┘
                                        │
                                        ▼
                                ┌─────────────────┐
                                │ Azure OpenAI    │
                                │ - GPT-4.1-mini  │
                                │ - Embeddings    │
                                └─────────────────┘
                                        │
                                        ▼
                              ┌─────────────────────┐
                              │ In-Memory Vector    │
                              │ Store (Community    │
                              │ Toolkit VectorData) │
                              └─────────────────────┘
```

### Core Services
- **Products Service**: Main API service with search endpoints and AI integration
- **Store Service**: Frontend web application for user interaction
- **SQL Server**: Database storage with Entity Framework integration
- **Azure OpenAI**: Chat completion and text embedding services

### Key Technologies
- **.NET Aspire**: Cloud-native orchestration and service discovery
- **Azure OpenAI**: GPT-4.1-mini for chat, text-embedding-ada-002 for embeddings
- **Microsoft.Extensions.AI (MEAI)**: `IChatClient` and `IEmbeddingGenerator` abstractions
- **CommunityToolkit.VectorData.InMemory**: In-memory vector store for semantic search
- **Entity Framework Core**: Data access and ORM
- **ASP.NET Core**: Web API and minimal APIs

## Configuration

The solution uses .NET Aspire's configuration system with support for:
- **User Secrets**: Local development with Azure OpenAI credentials
- **Azure Application Insights**: Telemetry and monitoring
- **Environment Variables**: Model deployment names and configuration

## Screenshots

### Aspire Dashboard
![Aspire Dashboard](./images/dashboard.jpg)

### Products Listing
![Products Listing](./images/products.jpg)

### Semantic Search
![Semantic Search](./images/search.jpg)

## Getting Started

1. Configure Azure OpenAI user secrets (see main README.md)
2. Run from the scenario's src directory:
   ```bash
   cd src/eShopAppHost
   dotnet run
   ```
3. Access the Aspire dashboard using the login URL provided in console output
4. Navigate to the Store application to test search functionality

## Related Resources

- [Main Scenario README](../README.md)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/cognitive-services/openai-service/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [CommunityToolkit.VectorData](https://www.nuget.org/packages/CommunityToolkit.VectorData.InMemory)