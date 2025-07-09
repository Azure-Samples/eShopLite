# DeepSeek-R1 AI Integration

## Overview
The DeepSeek scenario integrates with both Azure OpenAI and DeepSeek-R1 models to provide advanced reasoning capabilities and semantic search for enhanced eCommerce experiences.

## AI Models Configuration

### GPT-4.1 Mini Chat Model
```csharp
var chatDeploymentName = "gpt-41-mini";
var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: "gpt-4.1-mini",
        modelVersion: "2025-04-14");
gpt41mini.Resource.SkuCapacity = 10;
gpt41mini.Resource.SkuName = "GlobalStandard";
```

**Purpose**: Provides fast chat completion capabilities for quick product recommendations and customer interactions.

**Configuration**: 
- Model: `gpt-4.1-mini`
- Version: `2025-04-14`
- SKU: GlobalStandard with capacity of 10

### Text Embeddings Model
```csharp
var embeddingsDeploymentName = "text-embedding-ada-002";
var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
    modelName: "text-embedding-ada-002",
    modelVersion: "2");
```

**Purpose**: Generates vector embeddings for products and search queries to enable semantic search.

**Configuration**:
- Model: `text-embedding-ada-002`
- Version: `2`
- Used for vectorizing product descriptions and search terms

### DeepSeek-R1 Model
```csharp
var deepseekr1DeploymentName = "DeepSeek-R1";
```

**Purpose**: Provides advanced reasoning capabilities for complex product recommendations and sophisticated customer queries.

**Configuration**:
- Model: `DeepSeek-R1`
- Specialized for mathematical reasoning and complex problem solving
- Enhanced analytical capabilities for product comparisons

## Service Integration

### Products Service Configuration
```csharp
products.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

**Environment Variables**:
- `AI_ChatDeploymentName`: Set to "gpt-41-mini"
- `AI_embeddingsDeploymentName`: Set to "text-embedding-ada-002"

## DeepSeek-R1 Capabilities
- **Advanced Reasoning**: Complex logical analysis for product recommendations
- **Mathematical Problem Solving**: Enhanced calculation capabilities for pricing and comparisons
- **Multi-step Analysis**: Sophisticated product feature analysis
- **Chain-of-Thought Reasoning**: Detailed explanation of recommendations

## Multi-Model Strategy
The scenario employs a sophisticated AI strategy:
1. **GPT-4.1 Mini**: Fast responses for simple queries
2. **DeepSeek-R1**: Complex reasoning for advanced recommendations
3. **Text Embeddings**: Semantic understanding for search

## External Dependencies
- Azure OpenAI Service with multiple model deployments
- DeepSeek AI Foundry resource
- Azure Application Insights for telemetry
- Separate connection strings for different AI services