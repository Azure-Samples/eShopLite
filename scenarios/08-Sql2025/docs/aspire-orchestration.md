# Aspire Orchestration - SQL Server 2025

## Overview
The SQL Server 2025 scenario demonstrates an eCommerce application leveraging the latest SQL Server 2025 features including enhanced vector capabilities. It uses .NET Aspire to orchestrate services with modern database features.

## Services Configuration

### SQL Server 2025 Database
```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");
```

**Purpose**: Provides cutting-edge data storage with SQL Server 2025's enhanced vector and AI capabilities.

**Configuration**: 
- Uses SQL Server 2025 latest container image
- Persistent container lifetime for data retention
- Data volume for persistence
- Database name: `productsDb`
- Service name: `sql`
- EULA acceptance for SQL Server licensing

### Products Service
```csharp
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb);
```

**Purpose**: REST API that leverages SQL Server 2025's enhanced vector capabilities for advanced search operations.

**Dependencies**: 
- SQL Server 2025 database
- Waits for database to be ready before starting

### Store Web Application
```csharp
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();
```

**Purpose**: Blazor web application providing the eCommerce storefront with SQL Server 2025 enhanced features.

**Dependencies**: 
- Products service
- Exposed to external traffic via HTTP endpoints

## SQL Server 2025 Features
- **Enhanced Vector Support**: Native vector operations for semantic search
- **Improved Performance**: Optimized query execution for AI workloads
- **Advanced Indexing**: Better support for vector similarity searches
- **AI Integration**: Built-in capabilities for machine learning operations

## Container Configuration
- **Latest Image**: Uses the most recent SQL Server 2025 container
- **Persistent Lifetime**: Data survives container restarts
- **Volume Storage**: Persistent data storage configuration
- **Environment Variables**: Proper EULA acceptance

## External Dependencies
- SQL Server 2025 container runtime
- Azure OpenAI Service for AI capabilities
- Azure Application Insights (in production mode)