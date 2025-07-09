# Azure OpenAI Integration - SQL Server 2025

## Overview
The SQL Server 2025 scenario integrates Azure OpenAI services with SQL Server 2025's enhanced vector capabilities, creating a powerful combination for AI-powered eCommerce experiences.

## AI Models Configuration

### GPT-4.1 Mini Chat Model
```csharp
var chatDeploymentName = "gpt-41-mini";
var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: chatDeploymentName,
        modelVersion: "2025-04-14");
gpt41mini.Resource.SkuCapacity = 10;
gpt41mini.Resource.SkuName = "GlobalStandard";
```

**Purpose**: Provides intelligent chat completion capabilities that work seamlessly with SQL Server 2025's vector data.

**Configuration**: 
- Model: `gpt-41-mini`
- Version: `2025-04-14`
- SKU: GlobalStandard with capacity of 10

### Text Embeddings Model
```csharp
var embeddingsDeploymentName = "text-embedding-3-small";
var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
    modelName: embeddingsDeploymentName,
    modelVersion: "1");
```

**Purpose**: Generates high-quality vector embeddings that are stored and processed natively in SQL Server 2025.

**Configuration**:
- Model: `text-embedding-3-small`
- Version: `1`
- Optimized for performance and SQL Server 2025 compatibility

## Production vs Development Configuration

### Production Mode
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");
    // ... model deployments
    openai = aoai;
}
```

**Features**:
- Full Azure OpenAI service integration
- Application Insights telemetry
- Production-grade model deployments

### Development Mode
```csharp
else
{
    openai = builder.AddConnectionString("openai");
}
```

**Features**:
- Connection string-based configuration
- Flexible development setup
- User secrets support

## Service Integration

### Products Service Configuration
```csharp
products.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

**Environment Variables**:
- `AI_ChatDeploymentName`: Set to "gpt-41-mini"
- `AI_embeddingsDeploymentName`: Set to "text-embedding-3-small"

## Vector Integration Benefits

### Enhanced Performance
- **Database-Native Vectors**: Embeddings stored directly in SQL Server 2025
- **Optimized Queries**: Vector similarity searches using SQL Server's native capabilities
- **Reduced Latency**: No external vector database required

### Simplified Architecture
- **Single Data Store**: Both relational and vector data in SQL Server 2025
- **Consistent Transactions**: ACID compliance for vector operations
- **Unified Backup**: Single backup strategy for all data types

### Cost Optimization
- **Reduced Infrastructure**: Fewer external services required
- **Efficient Processing**: Hardware-optimized vector operations
- **Lower Latency**: Minimized data transfer between services

## External Dependencies
- Azure OpenAI Service with model deployments
- SQL Server 2025 with vector support
- Azure Application Insights (production mode)
- User secrets or connection strings for API keys