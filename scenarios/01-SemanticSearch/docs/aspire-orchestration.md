# Aspire Orchestration

## Overview
The Semantic Search scenario uses .NET Aspire to orchestrate a distributed eCommerce application with semantic search capabilities. The orchestration is handled through the AppHost project which coordinates SQL Server, Products API, and Store web application.

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

**Purpose**: Blazor web application providing the eCommerce storefront with semantic search UI.

**Dependencies**: 
- Products service
- Exposed to external traffic via HTTP endpoints

## External Dependencies
- SQL Server container
- Azure OpenAI Service (in production mode)
- Azure Application Insights (in production mode)