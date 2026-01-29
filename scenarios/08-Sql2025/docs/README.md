# eShopLite - SQL Server 2025 Vector Search Documentation

This documentation provides detailed technical information about the eShopLite SQL Server 2025 scenario, including native vector search capabilities, modern database features, and integrated AI services.

## Overview

The 08-Sql2025 scenario demonstrates cutting-edge database capabilities by leveraging SQL Server 2025's native vector search functionality. This scenario eliminates the need for external vector databases by implementing semantic search directly within SQL Server, showcasing the future of integrated AI-database solutions.

## Features Documentation

- [SQL Server 2025 Setup](./sql-server-2025-setup.md) - Container configuration and vector search enablement
- [Native Vector Search](./native-vector-search.md) - SQL Server 2025 vector operations and indexing
- [Azure OpenAI Integration](./azure-openai-integration.md) - Embedding generation and chat services
- [Entity Framework Vector Support](./entity-framework-vector-support.md) - EF Core integration with vector data types
- [Modern Database Features](./modern-database-features.md) - SQL Server 2025 enhancements and capabilities
- [Performance Optimization](./performance-optimization.md) - Vector indexing and query optimization

## Architecture

```
       ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
       │   Store (UI)    │───▶│  Products API   │───▶│ SQL Server 2025 │
       └─────────────────┘    └─────────────────┘    │ - Native Vector │
                                        │            │ - ACID Support  │
                                        │            └─────────────────┘
                                        ▼
                                ┌─────────────────┐
                                │ Azure OpenAI    │
                                │ - GPT-4.1-mini  │
                                │ - text-embed-   │
                                │   3-small       │
                                └─────────────────┘
                                        │
                                        ▼
                              ┌─────────────────────┐
                              │ Native Vector       │
                              │ Search & Storage    │
                              │ (In SQL Server)     │
                              └─────────────────────┘
```

### Core Services
- **Products Service**: API with native SQL vector search integration
- **Store Service**: Frontend web application
- **SQL Server 2025**: Database with native vector capabilities
- **Azure OpenAI**: Embedding generation with text-embedding-3-small

### Key Technologies
- **.NET Aspire**: Cloud-native orchestration
- **SQL Server 2025**: Latest version with native vector search
- **Entity Framework Core**: Vector data type support
- **Azure OpenAI**: text-embedding-3-small for embeddings
- **Microsoft.Extensions.AI**: Unified AI client abstractions

## SQL Server 2025 Features

### Native Vector Search
- **Built-in Vector Types**: Native support for vector data storage
- **Vector Indexes**: Optimized indexing for similarity search
- **SQL Vector Functions**: Native similarity search operations
- **Performance**: Hardware-optimized vector operations

### Container Configuration
```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")  // SQL Server 2025 preview
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");
```

## Vector Search Implementation

### Entity Framework Configuration
The scenario uses a custom DbContext configuration to enable vector search:

```csharp
// Custom configuration to enable vector search support
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));
```

### Embedding Generation
Modern AI service integration using Microsoft.Extensions.AI:

```csharp
var embeddingsDeploymentName = "text-embedding-3-small";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddEmbeddingGenerator(embeddingsDeploymentName);
```

## Model Configuration

### Text Embedding Model
- **Model**: text-embedding-3-small
- **Version**: 1
- **Dimensions**: Optimized for SQL Server vector operations
- **Purpose**: Generate embeddings for products and search queries

### Chat Model
- **Model**: gpt-4.1-mini
- **Version**: 2025-04-14
- **Purpose**: Conversational product assistance
- **Integration**: Microsoft.Extensions.AI chat client



## Screenshots

### Aspire Dashboard
![Aspire Dashboard](./images/dashboard.jpg)

### Products Listing
![Products Listing](./images/products.jpg)

### SQL Vector Search
![SQL Vector Search](./images/search.jpg)

## Key Advantages

### Native Database Integration
- **No External Vector DB**: All vector operations within SQL Server
- **ACID Compliance**: Full transactional support for vector data
- **Unified Storage**: Products and vectors in single database
- **Simplified Architecture**: Reduced infrastructure complexity

### Performance Benefits
- **Hardware Optimization**: SQL Server 2025 hardware-accelerated vectors
- **Intelligent Indexing**: Automatic vector index optimization
- **Query Optimization**: Native SQL vector query planning
- **Memory Management**: Efficient vector data caching

### Development Experience
- **Familiar Tools**: Standard SQL tools and practices
- **Entity Framework**: Native vector type support
- **Debugging**: Standard database debugging and profiling
- **Migration**: Easy migration from existing SQL Server deployments

## Configuration

### Environment Variables
- `AI_ChatDeploymentName`: "gpt-4.1-mini"
- `AI_embeddingsDeploymentName`: "text-embedding-3-small"

### Authentication
- **Development**: Azure CLI credential for local development
- **Production**: Managed Identity for secure Azure authentication

### Container Requirements
- **SQL Server 2025**: Latest preview image with vector support
- **Persistent Storage**: Data volume for database persistence
- **EULA Acceptance**: Required for SQL Server container licensing

## Getting Started

### Prerequisites
- Docker Desktop with SQL Server 2025 container support
- Azure OpenAI service with text-embedding-3-small model
- .NET 9.0 SDK

### Important Considerations

> **⚠️ Azure SQL Database Limitations**: When deploying to Azure SQL Database (as opposed to the containerized SQL Server 2025), be aware that vector indexes cannot be created inside stored procedures. See the [Native Vector Search documentation](./native-vector-search.md#known-issue-vector-indexes-in-stored-procedures-azure-sql-database) for detailed guidance on workarounds and best practices.

### Local Development
1. Configure Azure OpenAI user secrets
2. Ensure Docker is running
3. Run from the scenario's src directory:
   ```bash
   cd src/eShopAppHost
   dotnet run
   ```
4. SQL Server 2025 container will automatically download and start
5. Vector search capabilities will be automatically enabled

### Database Migration
The application automatically:
- Creates the database schema
- Initializes product data
- Generates and stores vector embeddings
- Creates vector indexes for optimal performance

## Related Resources

- [Main Scenario README](../README.md)
- [SQL Server 2025 Documentation](https://docs.microsoft.com/en-us/sql/relational-databases/vector-search/)
- [Entity Framework Vector Support](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai)
- [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/cognitive-services/openai-service/)