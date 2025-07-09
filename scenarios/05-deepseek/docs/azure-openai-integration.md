# Azure OpenAI Integration (DeepSeek Scenario)

## Overview

The 05-deepseek scenario combines Azure OpenAI services with the DeepSeek-R1 model to provide enhanced reasoning capabilities alongside traditional AI services. This dual-AI architecture maximizes the strengths of both platforms.

## Azure OpenAI Components

### GPT-4.1-mini
- **Model**: gpt-4.1-mini
- **Version**: 2025-04-14
- **Purpose**: Standard chat operations and product conversations
- **Capacity**: 10 units with GlobalStandard SKU

### Text Embeddings
- **Model**: text-embedding-ada-002
- **Version**: 2
- **Purpose**: Vector embeddings for semantic search
- **Integration**: Works alongside DeepSeek reasoning

## Service Configuration

### Azure OpenAI Setup
```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-41-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    var aoai = builder.AddAzureOpenAI("openai");
    
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-4.1-mini",
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "2");
}
```

### Environment Configuration
```csharp
products.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

## Dual AI Architecture

### Complementary Services
```
┌─────────────────────────────────────────┐
│              Products API               │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │ Azure OpenAI    │ │  DeepSeek-R1    │ │
│  │ - Chat (GPT-4.1)│ │ - Reasoning     │ │
│  │ - Embeddings    │ │ - Analysis      │ │
│  │ - Vector Search │ │ - Complex Logic │ │
│  └─────────────────┘ └─────────────────┘ │
│            │                │             │
│            ▼                ▼             │
│  ┌─────────────────────────────────────┐ │
│  │    Unified AI Response Engine       │ │
│  └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### Service Responsibilities

#### Azure OpenAI Tasks
- **Vector Embeddings**: Generate embeddings for products and queries
- **Standard Chat**: Handle routine product inquiries
- **Similarity Search**: Find semantically similar products
- **Quick Responses**: Fast responses for simple queries

#### DeepSeek-R1 Tasks
- **Complex Reasoning**: Multi-step logical analysis
- **Feature Analysis**: Deep understanding of product relationships
- **Comparative Analysis**: Advanced product comparisons
- **Intent Disambiguation**: Clarify complex or ambiguous requests

## Integration Patterns

### Intelligent Routing
```csharp
public async Task<SearchResult> ProcessQuery(string userQuery)
{
    // Determine complexity of query
    var complexity = await AnalyzeQueryComplexity(userQuery);
    
    if (complexity == QueryComplexity.Simple)
    {
        // Use Azure OpenAI for fast response
        return await azureOpenAIService.ProcessQuery(userQuery);
    }
    else
    {
        // Use DeepSeek-R1 for complex reasoning
        var reasoningResult = await deepSeekService.ReasonAboutQuery(userQuery);
        
        // Combine with Azure OpenAI embeddings for product matching
        var embeddings = await azureOpenAIService.GenerateEmbeddings(reasoningResult.ProcessedQuery);
        
        return await CombineResults(reasoningResult, embeddings);
    }
}
```

### Fallback Strategies
```csharp
public async Task<Response> GetAIResponse(string query)
{
    try
    {
        // Try DeepSeek first for complex reasoning
        var deepSeekResponse = await deepSeekR1Client.ReasonAsync(query);
        if (deepSeekResponse.IsSuccessful)
        {
            return deepSeekResponse;
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "DeepSeek service unavailable, falling back to Azure OpenAI");
    }
    
    // Fallback to Azure OpenAI
    return await azureOpenAIClient.ChatAsync(query);
}
```

## Performance Optimization

### Caching Strategy
```csharp
public class DualAICache
{
    private readonly IMemoryCache _cache;
    
    public async Task<SearchResult> GetCachedResult(string query)
    {
        var cacheKey = $"reasoning:{query.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out SearchResult cachedResult))
        {
            return cachedResult;
        }
        
        // Process with appropriate AI service
        var result = await ProcessWithOptimalService(query);
        
        // Cache complex reasoning results longer
        var expiration = result.RequiredReasoning ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(15);
        _cache.Set(cacheKey, result, expiration);
        
        return result;
    }
}
```

### Load Balancing
- **Query Classification**: Route based on complexity analysis
- **Service Health**: Monitor both AI services for availability
- **Cost Optimization**: Use appropriate service for cost-effectiveness
- **Response Time**: Balance quality vs speed based on user needs

## Use Case Examples

### Simple Queries (Azure OpenAI)
```
User: "Show me laptops"
Service: Azure OpenAI (GPT-4.1-mini)
Response: Quick product list with embeddings-based ranking
```

### Complex Queries (DeepSeek-R1 + Azure OpenAI)
```
User: "I need a laptop for machine learning that's portable but powerful enough for training models, under $2000"
Service: DeepSeek-R1 (reasoning) + Azure OpenAI (embeddings)
Response: Multi-step analysis of requirements with detailed product recommendations
```

### Hybrid Processing
```
User: "Compare gaming laptops for streaming"
Flow:
1. DeepSeek-R1: Analyze "gaming" + "streaming" requirements
2. Azure OpenAI: Generate embeddings for refined search criteria
3. Combined: Merge reasoning insights with vector similarity
4. Result: Comprehensive comparison with reasoning explanation
```

## Configuration Management

### Local Development
```bash
# Azure OpenAI connection
cd src/Products
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<key>;"

# DeepSeek connection (if available locally)
dotnet user-secrets set "ConnectionStrings:deepseekr1" "Endpoint=https://<endpoint>;Key=<key>;"
```

### Production Deployment
- **Azure OpenAI**: Provisioned via Aspire with proper RBAC
- **DeepSeek-R1**: Deployed via Bicep templates with managed identity
- **Unified Configuration**: Environment variables for service coordination
- **Health Monitoring**: Comprehensive monitoring for both services

## Error Handling and Resilience

### Multi-Service Resilience
```csharp
public class AIServiceOrchestrator
{
    public async Task<Response> ProcessRequest(string query)
    {
        var services = new List<IAIService> { deepSeekService, azureOpenAIService };
        
        foreach (var service in services)
        {
            try
            {
                var result = await service.ProcessAsync(query);
                if (result.IsValid) return result;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Service {ServiceName} failed", service.Name);
                continue;
            }
        }
        
        return new Response { Message = "AI services temporarily unavailable" };
    }
}
```

### Graceful Degradation
- **Service Outages**: Automatic failover between services
- **Partial Functionality**: Maintain core search when advanced reasoning unavailable
- **User Communication**: Clear messaging about service limitations
- **Recovery Monitoring**: Automatic service restoration detection