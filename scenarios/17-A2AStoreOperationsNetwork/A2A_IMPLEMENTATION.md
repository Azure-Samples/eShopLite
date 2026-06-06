# A2A eShopLite Demo - Implementation Documentation

## Overview

This project demonstrates a practical Agent2Agent (A2A) scenario using the eShopLite sample as a foundation. The implementation showcases how multiple autonomous agents (microservices) can collaborate to fulfill complex business requests through the official Microsoft A2A .NET SDK.

## Architecture

### Components

1. **Store (Frontend)** - Blazor web application providing the user interface
2. **Products API** - Main backend API that orchestrates agent communication using A2A .NET SDK
3. **Three Autonomous Agents:**
   - **Inventory Agent** - Provides real-time stock levels via A2A SDK message handling
   - **Promotions Agent** - Supplies active promotions and discounts via A2A SDK message handling  
   - **Researcher Agent** - Delivers product insights and reviews via A2A SDK message handling

### A2A Orchestration Flow

When a user selects "A2A Search" and submits a query:

1. **Products API** receives the search request via `/api/a2asearch/{search}`
2. **Products API** identifies relevant products using standard search
3. For each product, **Products API** orchestrates calls using A2A SDK:
   - **Inventory Agent** message handler (`HandleInventoryCheckAsync`) - Gets stock levels
   - **Promotions Agent** message handler (`HandlePromotionsAsync`) - Gets active promotions
   - **Researcher Agent** message handler (`HandleInsightsAsync`) - Gets product insights
4. **Products API** aggregates all responses into a unified result
5. **Store Frontend** displays enriched product information

## Implementation Details

### A2A .NET SDK Integration

The implementation uses A2A package (v0.1.0-preview.2) for agent management:

#### Agent Skills Definition
Each agent defines its capabilities using AgentSkill:
```csharp
_skill = new AgentSkill
{
    Id = "check_inventory",
    Name = "Check Inventory",
    Description = "Check inventory levels for a product",
    Tags = new List<string> { "inventory", "stock", "product" },
    Examples = new List<string> { "Check stock for product 123" },
    InputModes = new List<string> { "text" },
    OutputModes = new List<string> { "json" }
};
```

#### Message Handling Pattern
Agents handle A2A messages using the Message class:
```csharp
public async Task<string> HandleInventoryCheckAsync(Message message)
{
    // Extract product ID from message parts
    var textPart = message.Parts?.OfType<TextPart>().FirstOrDefault();
    var productId = ExtractProductIdFromMessage(textPart.Text);
    
    // Process request and return JSON response
    var result = await CheckInventoryAsync(productId);
    return JsonSerializer.Serialize(result);
}
```

### A2A Orchestration Service

The `A2AOrchestrationService` has been updated to use the A2A .NET SDK:

```csharp
public class A2AOrchestrationService : IA2AOrchestrationService
{
    private readonly InventoryAgent _inventoryAgent;
    private readonly PromotionsAgent _promotionsAgent;
    private readonly ResearcherAgent _researcherAgent;
    
    // Orchestrates agent calls using A2A SDK message patterns
    public async Task<A2ASearchResponse> ExecuteA2ASearchAsync(string searchTerm)
    {
        // Create A2A messages for agent communication
        var productMessage = CreateProductMessage(product.Id.ToString());
        
        // Parallel agent message handler calls
        var inventoryTask = _inventoryAgent.HandleInventoryCheckAsync(productMessage);
        var promotionsTask = _promotionsAgent.HandlePromotionsAsync(productMessage);
        var insightsTask = _researcherAgent.HandleInsightsAsync(productMessage);
        
        await Task.WhenAll(inventoryTask, promotionsTask, insightsTask);
        
        // Parse JSON responses and aggregate results
        var inventoryResponse = JsonSerializer.Deserialize<InventoryResponse>(await inventoryTask);
        var promotionsResponse = JsonSerializer.Deserialize<PromotionsResponse>(await promotionsTask);
        var insightsResponse = JsonSerializer.Deserialize<ResearchResponse>(await insightsTask);
    }
    
    private Message CreateProductMessage(string productId)
    {
        return new Message
        {
            Parts = new List<Part>
            {
                new TextPart { Text = JsonSerializer.Serialize(new { productId = productId }) }
            },
            Role = MessageRole.User
        };
    }
}
```

### NuGet Packages

The implementation leverages the following key package:

- **A2A** (v0.1.0-preview.2) - Provides the official Microsoft A2A .NET SDK for agent-to-agent communication

### Service Registration

```csharp
// Configure HttpClients for agents
builder.Services.AddHttpClient("InventoryAgent", client =>
{
    client.BaseAddress = new Uri("http://inventory-agent");
});

// Add A2A Agents using A2A .NET SDK
builder.Services.AddScoped<InventoryAgent>();
builder.Services.AddScoped<PromotionsAgent>();
builder.Services.AddScoped<ResearcherAgent>();

// Add A2A Orchestration Service
builder.Services.AddScoped<IA2AOrchestrationService, A2AOrchestrationService>();
```

### Agent Endpoints

The agents expose HTTP endpoints for external communication and handle A2A SDK messages internally:

#### Inventory Agent
- **HTTP Endpoint:** `POST /api/inventory/check`
- **A2A Message Handler:** `HandleInventoryCheckAsync`
- **Agent Skill ID:** `check_inventory`
- **Request:** `{ "productId": "string" }`
- **Response:** `{ "productId": "string", "stock": 42 }`

#### Promotions Agent
- **HTTP Endpoint:** `POST /api/promotions/active`
- **A2A Message Handler:** `HandlePromotionsAsync`
- **Agent Skill ID:** `get_promotions`
- **Request:** `{ "productId": "string" }`
- **Response:** `{ "productId": "string", "promotions": [{ "title": "string", "discount": 10 }] }`

#### Researcher Agent
- **HTTP Endpoint:** `POST /api/researcher/insights`
- **A2A Message Handler:** `HandleInsightsAsync`
- **Agent Skill ID:** `get_insights`
- **Request:** `{ "productId": "string" }`
- **Response:** `{ "productId": "string", "insights": [{ "review": "string", "rating": 4.5 }] }`

### Frontend Features

The Store application includes a search page with:
- **Search Type Selector**: Dropdown with options:
  - Standard Search
  - Semantic Search  
  - A2A Search (Agent-to-Agent)
- **Enhanced Results Display**: Shows enriched data from all agents for A2A searches

### Data Models

#### A2A Enriched Product Response
```json
{
  "response": "Found X products enriched with inventory, promotions, and insights data.",
  "products": [
    {
      "productId": "1",
      "name": "Product Name",
      "description": "Product Description",
      "price": 99.99,
      "imageUrl": "image.jpg",
      "stock": 42,
      "promotions": [
        {
          "title": "Special Offer",
          "discount": 15
        }
      ],
      "insights": [
        {
          "review": "Great product!",
          "rating": 4.5
        }
      ]
    }
  ]
}
```

## Project Structure

```
src/
├── eShopAppHost/              # Aspire orchestration host
├── eShopServiceDefaults/      # Shared Aspire service configurations
├── Store/                     # Blazor frontend application
├── Products/                  # Main Products API with A2A orchestration
│   ├── Services/             # A2A orchestration service
│   │   └── Agents/           # A2A SDK Agent implementations
│   └── Endpoints/            # API endpoints including A2ASearch
├── InventoryAgent/           # Inventory management agent
├── PromotionsAgent/          # Promotions management agent
├── ResearcherAgent/          # Product research agent
├── DataEntities/             # Shared data models
├── SearchEntities/           # Search and A2A response models
└── Products.Tests/           # Unit tests including A2A orchestration tests
```

## Key Features Implemented

- ✅ Three autonomous agent projects with dedicated APIs
- ✅ **A2A .NET SDK integration** for agent-to-agent orchestration
- ✅ **AgentSkill definitions** for structured agent capabilities
- ✅ **A2A package (v0.1.0-preview.2)** integration
- ✅ Enhanced frontend with search type selection
- ✅ Aspire integration for service orchestration
- ✅ Unit tests for A2A orchestration functionality with agent mocking
- ✅ Error handling and graceful degradation
- ✅ Parallel agent calls for optimal performance

## Testing

The implementation includes comprehensive unit tests updated for A2A .NET SDK:
- A2A orchestration service tests with mocked agent responses using the A2A SDK patterns
- Agent dependency injection and testing with proper logger mocking
- Existing product API tests continue to pass
- All tests validate proper error handling and data aggregation through agent message handlers

Example test setup:
```csharp
var inventoryAgent = new InventoryAgent(httpClientFactory, _inventoryLogger);
var promotionsAgent = new PromotionsAgent(httpClientFactory, _promotionsLogger);
var researchAgent = new ResearcherAgent(httpClientFactory, _researchLogger);

var orchestrationService = new A2AOrchestrationService(
    context, inventoryAgent, promotionsAgent, researchAgent, _logger);
```

## Usage

1. Start the Aspire AppHost: `dotnet run` in `eShopAppHost` directory
2. Navigate to the Store application
3. Go to the Search page
4. Select "A2A Search (Agent-to-Agent)" from the dropdown
5. Enter a search term and click Search
6. View enriched results with inventory, promotions, and insights data

## Performance Considerations

- Agent calls are made in parallel to minimize response time
- Graceful handling of agent failures (partial results returned)
- Response caching can be added for production scenarios
- Circuit breaker patterns can be implemented for resilience

## Future Enhancements

- Dynamic agent discovery and registration using A2A SDK
- Advanced failure handling and retry policies within agent message handlers
- Real-time agent health monitoring through the A2A SDK framework
- Enhanced A2A SDK integration for standardized agent communication
- Agent authentication and authorization through A2A SDK security features
- Advanced agent orchestration patterns using A2A SDK capabilities