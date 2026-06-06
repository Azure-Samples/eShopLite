# Aspire Orchestration

## Overview

The eShopLite Semantic Search scenario uses .NET Aspire for service orchestration, providing a cloud-native approach to manage dependencies, configuration, and deployment across development and production environments.

## Service Architecture

### Core Services Configuration

The AppHost `Program.cs` defines the following service topology:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// SQL Server with persistent data volume
var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

// Products API service with SQL dependency
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb);

// Store frontend with Products API dependency
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();
```

### Production vs Development Configuration

The orchestration uses conditional configuration based on execution context:

#### Development Mode
- Uses connection strings for Azure OpenAI
- Relies on user secrets for configuration
- Local SQL Server container

#### Production Mode (Azure)
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");
    
    // Configure chat model deployment
    var gpt41mini = aoai.AddDeployment(
        name: "gpt-41-mini",
        modelName: "gpt-4.1-mini", 
        modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    // Configure embeddings deployment  
    var embeddingsDeployment = aoai.AddDeployment(
        name: "text-embedding-ada-002",
        modelName: "text-embedding-ada-002",
        modelVersion: "2");
}
```

## Service Dependencies

### Dependency Graph
```
Store (Frontend)
  └── Products (API)
      ├── SQL Server Database
      └── Azure OpenAI (Chat + Embeddings)
```

### Wait Conditions
- Products service waits for SQL database to be ready
- Store service waits for Products API to be available
- Ensures proper startup order and health checks

## Configuration Management

### Environment Variables
- `AI_ChatDeploymentName`: "gpt-41-mini"
- `AI_embeddingsDeploymentName`: "text-embedding-ada-002"

### Service References
- All services automatically receive service discovery URLs
- Connection strings are injected via Aspire's configuration system
- Health checks and telemetry are configured automatically

## Azure Resources

When deployed to Azure via `azd up`:
- **Azure Container Apps Environment**: Hosts all services
- **Azure Application Insights**: Centralized telemetry and monitoring
- **Azure OpenAI**: Managed AI services with model deployments
- **SQL Database**: Managed database service
- **Container Registry**: Stores application container images
- **Managed Identity**: Secure service-to-service authentication

## Benefits

1. **Service Discovery**: Automatic URL resolution between services
2. **Configuration Management**: Centralized secrets and settings
3. **Observability**: Built-in telemetry and health monitoring
4. **Scalability**: Container-based deployment with Azure Container Apps
5. **Security**: Managed identities and secure credential management