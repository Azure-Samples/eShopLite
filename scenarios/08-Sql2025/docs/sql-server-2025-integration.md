# SQL Server 2025 Integration

## Overview

The 08-Sql2025 scenario demonstrates integration with SQL Server 2025, showcasing the latest database features and capabilities combined with modern AI search functionality. This integration highlights SQL Server 2025's enhanced performance, vector support, and cloud-native optimizations.

## Key Features

### SQL Server 2025 Container
- **Image**: `microsoft/mssql-server:2025-latest`
- **Container Management**: Persistent container lifecycle
- **Data Persistence**: Volume-backed storage for production scenarios
- **EULA Compliance**: Automatic license acceptance configuration

### Enhanced Database Capabilities
- **Performance Improvements**: Latest SQL Server optimizations
- **Vector Search Integration**: Native vector storage and similarity search
- **Modern Data Types**: Support for advanced data structures
- **Cloud-Native Features**: Optimized for containerized environments

## Configuration Implementation

### AppHost Container Setup

```csharp
// SQL Server 2025 container configuration
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");

// Products service with database reference
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb);
```

### Database Context Configuration

The Products service leverages Entity Framework Core with SQL Server 2025:

```csharp
// Entity Framework with SQL Server 2025
builder.AddSqlServerDbContext<Context>("productsDb");
```

### Vector Search Integration

```csharp
// EF Core Vector Search package
<PackageReference Include="EFCore.SqlServer.VectorSearch" Version="9.0.0-preview.2" />
```

## SQL Server 2025 Features

### Container Lifecycle Management
- **Persistent Containers**: `ContainerLifetime.Persistent` ensures containers survive restarts
- **Data Volumes**: Dedicated volume storage for database files
- **Environment Configuration**: Proper SQL Server environment setup

### Database Provisioning
```csharp
var productsDb = sql
    .WithDataVolume()        // Persistent storage
    .AddDatabase("productsDb"); // Named database creation
```

### Advanced Configuration Options
- **EULA Acceptance**: Automatic license agreement (`ACCEPT_EULA=Y`)
- **Latest Features**: Access to cutting-edge SQL Server capabilities
- **Container Optimization**: Optimized for containerized deployment patterns

## Integration Architecture

### Service Dependencies
```
┌─────────────────────────────────────────┐
│              Products API               │
├─────────────────────────────────────────┤
│                                         │
│  Entity Framework Core Integration      │
│                                         │
│  ┌─────────────────────────────────────┐ │
│  │         SQL Server 2025             │ │
│  │ ┌─────────────────────────────────┐ │ │
│  │ │       Products Database         │ │ │
│  │ │ - Product Entities              │ │ │
│  │ │ - Vector Storage                │ │ │
│  │ │ - Search Indexes                │ │ │
│  │ └─────────────────────────────────┘ │ │
│  └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### Data Layer Integration
- **Entity Framework Core**: Modern ORM with SQL Server 2025 optimizations
- **Vector Search**: Native vector similarity search capabilities
- **Performance**: Optimized queries and connection pooling
- **Transactions**: ACID compliance with modern transaction management

## Vector Search Capabilities

### SQL Server 2025 Vector Features
- **Native Vector Storage**: Built-in vector data types
- **Similarity Search**: Optimized vector similarity operations
- **Index Management**: Efficient vector indexing strategies
- **Query Optimization**: Enhanced query plans for vector operations

### EF Core Vector Integration
```csharp
// Vector search with EF Core
using EFCore.SqlServer.VectorSearch;

// Vector similarity queries
var similarProducts = await context.Products
    .Where(p => p.EmbeddingVector.CosineSimilarity(queryVector) > threshold)
    .OrderByDescending(p => p.EmbeddingVector.CosineSimilarity(queryVector))
    .Take(10)
    .ToListAsync();
```

## Development and Production Modes

### Local Development
```csharp
// Local development with connection strings
if (!builder.ExecutionContext.IsPublishMode)
{
    openai = builder.AddConnectionString("openai");
}

products.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

### Production Deployment
```csharp
// Production with Azure services
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");
    
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: chatDeploymentName,
            modelVersion: "2025-04-14");
    
    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: embeddingsDeploymentName,
        modelVersion: "1");
    
    products.WithReference(appInsights)
        .WithReference(aoai);
}
```

## Performance Optimizations

### SQL Server 2025 Enhancements
- **Query Optimizer**: Latest query optimization algorithms
- **Memory Management**: Enhanced buffer pool and memory allocation
- **I/O Improvements**: Optimized disk I/O patterns
- **Parallel Processing**: Enhanced parallel query execution

### Container Performance
- **Resource Allocation**: Optimal container resource configuration
- **Volume Performance**: High-performance volume drivers
- **Network Optimization**: Efficient container networking
- **Startup Time**: Fast container initialization

## Monitoring and Observability

### Aspire Integration
- **Health Checks**: Comprehensive database health monitoring
- **Telemetry**: Detailed performance and usage metrics
- **Logging**: Structured logging for database operations
- **Tracing**: Distributed tracing across service boundaries

### SQL Server Monitoring
- **Query Performance**: Query execution statistics
- **Resource Usage**: CPU, memory, and I/O monitoring
- **Connection Pooling**: Connection pool health and metrics
- **Error Tracking**: Database error detection and alerting

## Migration and Compatibility

### Upgrading from Previous Versions
- **Schema Migration**: EF Core migrations for schema changes
- **Data Migration**: Strategies for existing data
- **Feature Adoption**: Gradual adoption of SQL Server 2025 features
- **Backward Compatibility**: Ensuring compatibility with existing applications

### Best Practices
- **Connection Management**: Efficient connection pooling
- **Transaction Patterns**: Optimal transaction boundaries
- **Error Handling**: Robust error recovery strategies
- **Security**: Latest security features and configurations