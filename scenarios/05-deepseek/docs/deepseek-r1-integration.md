# DeepSeek-R1 Integration

## Overview

The 05-deepseek scenario integrates the advanced DeepSeek-R1 reasoning model to provide enhanced semantic search and product analysis capabilities. DeepSeek-R1 brings advanced reasoning abilities that complement Azure OpenAI services for superior search relevance.

## Key Features

### Advanced Reasoning Model
- **Model**: DeepSeek-R1
- **Version**: 1
- **Purpose**: Advanced logical reasoning for complex product queries and analysis
- **Capabilities**: Multi-step reasoning, contextual understanding, and improved search relevance

### Enhanced Search Intelligence
- **Complex Query Processing**: Handles multi-faceted product search requests
- **Intent Understanding**: Better comprehension of user search intent
- **Feature Analysis**: Deep understanding of product features and relationships
- **Reasoning Chain**: Transparent reasoning process for search results

## Infrastructure Configuration

### Azure Bicep Template

```bicep
var aiServicesNameAndSubdomain = '${rgname}-aiservices'
module aiServices 'br/public:avm/res/cognitive-services/account:0.9.2' = {
  name: 'DeepSeek-R1'
  scope: resourceGroup()
  params: {
    name: aiServicesNameAndSubdomain
    location: location
    tags: tags
    kind: 'AIServices'
    customSubDomainName: aiServicesNameAndSubdomain
    sku: 'S0'
    publicNetworkAccess: 'Enabled'
    deployments: [
      {
        name: 'DeepSeek-R1'
        model: {
          format: 'DeepSeek'
          name: 'DeepSeek-R1'
          version: '1'
        }
        sku: {
          name: 'Standard'
          capacity: 100
        }
      }
    ]
    roleAssignments: [
      {
        principalId: principalId
        principalType: principalType
        roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd')
      }
    ]
  }
}
```

### Service Configuration

```csharp
// DeepSeek R1 deployment configuration
var deepseekr1DeploymentName = "DeepSeek-R1";

// In production mode with Azure services
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-41-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    
    // Azure OpenAI for embeddings and standard chat
    var aoai = builder.AddAzureOpenAI("openai");
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-4.1-mini",
            modelVersion: "2025-04-14");
    
    // DeepSeek R1 for advanced reasoning
    // Connected via separate AI services deployment
    
    products.WithReference(appInsights)
            .WithReference(aoai)
            .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
            .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
}
```

## Integration Architecture

### Dual AI Service Pattern
```
┌─────────────────────────────────────────┐
│              Products API               │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │ Azure OpenAI    │ │  DeepSeek-R1    │ │
│  │ - Embeddings    │ │ - Reasoning     │ │
│  │ - GPT-4.1-mini  │ │ - Analysis      │ │
│  └─────────────────┘ └─────────────────┘ │
│                                         │
└─────────────────────────────────────────┘
```

### Service Responsibilities
- **Azure OpenAI**: Handles vector embeddings and standard conversational AI
- **DeepSeek-R1**: Provides advanced reasoning for complex search scenarios
- **Combined Intelligence**: Both services work together for optimal results

## Deployment Features

### Resource Provisioning
- **Automated Deployment**: Complete infrastructure via Bicep templates
- **IAM Configuration**: Proper role assignments for service communication
- **Managed Identity**: Secure authentication between services
- **Scaling**: Standard SKU with 100 capacity units

### Security Configuration
```bicep
roleAssignments: [
  {
    principalId: principalId
    principalType: principalType
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd')
  }
]
```

## Use Cases

### Enhanced Search Scenarios
1. **Complex Product Queries**: "Find waterproof jackets suitable for hiking in cold weather"
2. **Feature-based Search**: "Products that are eco-friendly and under $50"
3. **Comparative Analysis**: "Compare wireless headphones for gaming vs music"
4. **Contextual Recommendations**: "Suggest camping gear for a family of four"

### Reasoning Capabilities
- **Multi-step Analysis**: Breaking down complex requirements
- **Feature Correlation**: Understanding relationships between product attributes
- **Intent Disambiguation**: Clarifying ambiguous search terms
- **Context Awareness**: Maintaining conversation context across interactions

## Performance Considerations

### Model Optimization
- **Reasoning Efficiency**: Optimized for fast reasoning operations
- **Capacity Planning**: 100 units for concurrent reasoning requests
- **Caching Strategy**: Results caching for repeated reasoning patterns
- **Fallback Mechanism**: Azure OpenAI fallback for availability

### Cost Management
- **Usage Monitoring**: Track reasoning API usage
- **Intelligent Routing**: Use DeepSeek-R1 only for complex queries
- **Optimization**: Balance between reasoning quality and cost

## Configuration Requirements

### Local Development
Configure connection to DeepSeek services via user secrets:
```bash
cd src/Products  
dotnet user-secrets set "ConnectionStrings:deepseekr1" "Endpoint=https://<endpoint>;Key=<key>;"
```

### Production Environment
- Automatic provisioning via Bicep templates
- Managed identity authentication
- Environment variable injection via Aspire AppHost
- Proper resource tagging and governance

## Error Handling

### Resilience Patterns
- **Circuit Breaker**: Protect against DeepSeek service failures
- **Retry Logic**: Intelligent retry for transient failures
- **Fallback Strategy**: Use Azure OpenAI when DeepSeek unavailable
- **Monitoring**: Comprehensive telemetry and alerting

### Graceful Degradation
- Standard search when reasoning services are down
- Partial results with clear user messaging
- Service health monitoring and status reporting