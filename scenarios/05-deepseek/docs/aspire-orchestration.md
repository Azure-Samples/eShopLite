# Aspire Orchestration - DeepSeek

## Overview
The DeepSeek scenario demonstrates an eCommerce application enhanced with DeepSeek-R1 AI capabilities. It uses .NET Aspire to orchestrate services with advanced AI reasoning and semantic search features.

## Services Configuration

### SQL Server Database
```csharp
var sqldb = builder.AddSqlServer("sql")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume()
            .AddDatabase("sqldb");
```

**Purpose**: Provides persistent data storage for products and application data.

**Configuration**: 
- Uses persistent container lifetime for data retention
- Uses data volume for persistence
- Database name: `sqldb`
- Service name: `sql`

### Products Service
```csharp
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithExternalHttpEndpoints();
```

**Purpose**: REST API that handles product data and advanced AI-powered search operations.

**Dependencies**: 
- SQL Server database
- Waits for database to be ready before starting
- Exposed to external traffic for API access

### Store Web Application
```csharp
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();
```

**Purpose**: Blazor web application providing the eCommerce storefront with DeepSeek AI integration.

**Dependencies**: 
- Products service
- Exposed to external traffic via HTTP endpoints

## Container Configuration
- **Persistent Lifetime**: SQL Server maintains data across application restarts
- **External Endpoints**: Both Products and Store services are accessible externally
- **Service Dependencies**: Clear dependency chain ensures proper startup order

## External Dependencies
- SQL Server container with persistent storage
- Azure OpenAI Service with DeepSeek-R1 model
- Azure Application Insights (in production mode)