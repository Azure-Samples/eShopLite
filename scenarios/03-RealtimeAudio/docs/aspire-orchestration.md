# Aspire Orchestration - Realtime Audio

## Overview
The Realtime Audio scenario extends the basic eCommerce application with real-time audio interaction capabilities. It uses .NET Aspire to orchestrate multiple services including a specialized realtime audio store.

## Services Configuration

### SQL Server Database
```csharp
var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");
```

**Purpose**: Provides persistent data storage for products and application data.

**Configuration**: 
- Uses data volume for persistence
- Database name: `sqldb`
- Service name: `sql`

### Products Service
```csharp
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb);
```

**Purpose**: REST API that handles product data and semantic search operations.

**Dependencies**: 
- SQL Server database
- Waits for database to be ready before starting

### Store Web Application
```csharp
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();
```

**Purpose**: Traditional Blazor web application providing the eCommerce storefront.

**Dependencies**: 
- Products service
- Exposed to external traffic via HTTP endpoints

### Realtime Store Application
```csharp
var storeRealtime = builder.AddProject<Projects.StoreRealtime>("realtimestore")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();
```

**Purpose**: Specialized Blazor web application with real-time audio chat capabilities for product recommendations.

**Dependencies**: 
- Products service
- Exposed to external traffic via HTTP endpoints
- Integrates with OpenAI real-time API for audio conversations

## External Dependencies
- SQL Server container
- Azure OpenAI Service with real-time models
- Azure Application Insights (in production mode)