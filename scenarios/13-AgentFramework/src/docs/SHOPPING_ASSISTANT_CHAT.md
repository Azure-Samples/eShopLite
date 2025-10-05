# Shopping Assistant Chat Implementation

## Overview
This implementation demonstrates a complete chat interface for the Shopping Assistant Agent in the eShopLite Blazor application.

## Components Implemented

### 1. Backend Services

#### Chat Models (`Store/Services/IShoppingAgentChatService.cs`)
- `ChatMessage`: Record type for individual messages (role, content, timestamp)
- `ChatRequest`: Request payload sent to agent
- `ChatResponse`: Response from agent including conversation ID and optional product cards
- `ProductCard`: Product information that can be returned by the agent

#### Chat Service (`Store/Services/ShoppingAgentChatService.cs`)
- `IShoppingAgentChatService`: Interface for chat operations
- `ShoppingAgentChatService`: Implementation that calls `/api/agent/chat` endpoint
- Handles conversation state management via ConversationId
- Includes error handling and logging

### 2. Frontend Components

#### Assistant Page (`Store/Components/Pages/Assistant.razor`)
- Interactive Blazor Server component
- Full chat UI with:
  - Message transcript with auto-scroll
  - User/assistant message differentiation
  - Typing indicator during API calls
  - Error message display
  - Conversation state management
  - Responsive design

#### Navigation (`Store/Components/Layout/NavMenu.razor`)
- Added "Assistant" link with chat icon

### 3. Service Registration (`Store/Program.cs`)
```csharp
builder.Services.AddScoped<IShoppingAgentChatService, ShoppingAgentChatService>();
builder.Services.AddHttpClient<IShoppingAgentChatService, ShoppingAgentChatService>(
    static client => client.BaseAddress = new("https+http://shopping-agent"));
```

## Features

### Chat Interface
- ? Real-time message exchange
- ? Conversation history maintained
- ? Auto-scrolling transcript
- ? Loading states with typing indicator
- ? Error handling with user-friendly messages
- ? Responsive mobile-friendly design
- ? Bootstrap Icons integration

### Agent Integration
- ? HTTP POST to `/api/agent/chat`
- ? Conversation ID tracking
- ? Message history sent with each request
- ? Extensible for product cards display

## Usage

1. **Start the Application**
   ```bash
   cd eShopAppHost
   dotnet run
   ```

2. **Navigate to Assistant**
   - Click "Assistant" in the navigation menu
   - Or navigate to `/assistant`

3. **Chat with the Assistant**
   - Type a message in the input field
   - Press Enter or click Send
   - Messages appear in the transcript
   - Conversation context is maintained

## API Contract

### Request
```json
{
  "message": "Find me some shoes",
  "conversationId": "abc-123",
  "history": [
    {
      "role": "user",
      "content": "Hello",
      "timestamp": "2024-01-15T10:30:00Z"
    },
    {
      "role": "assistant",
      "content": "Hi! How can I help?",
      "timestamp": "2024-01-15T10:30:01Z"
    }
  ]
}
```

### Response
```json
{
  "message": "I found some great shoes for you!",
  "conversationId": "abc-123",
  "products": [
    {
      "id": "1",
      "name": "Running Shoes",
      "price": 89.99,
      "imageUrl": "/images/shoes.jpg",
      "description": "Comfortable running shoes"
    }
  ]
}
```

## Future Enhancements

### Planned Features
- [ ] Display product cards in chat
- [ ] Add quick action buttons (Add to Cart, View Details)
- [ ] Implement streaming responses (SSE)
- [ ] Add conversation persistence (localStorage)
- [ ] Show suggested questions/prompts
- [ ] Voice input support
- [ ] File/image upload
- [ ] Rich message formatting (markdown)

### Agent Improvements
- [ ] Integrate with SearchCatalogTool
- [ ] Integrate with ProductDetailsTool
- [ ] Integrate with AddToCartTool
- [ ] Add function calling via Microsoft.Agents.Client
- [ ] Implement multi-turn reasoning
- [ ] Add conversation memory

## Testing

### Manual Testing Scenarios
1. **Basic Chat**
   - Send: "Hello"
   - Verify: Assistant responds

2. **Product Search**
   - Send: "Show me shoes"
   - Verify: Response includes product information

3. **Conversation Context**
   - Send multiple related messages
   - Verify: Assistant maintains context

4. **Error Handling**
   - Stop ShoppingAssistantAgent service
   - Send message
   - Verify: Error message displayed

### Unit Testing
```csharp
// Example test structure (to be implemented)
[Fact]
public async Task SendAsync_ValidMessage_ReturnsResponse()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("/api/agent/chat")
           .Respond("application/json", "{\"message\":\"Test response\"}");
    
    var service = new ShoppingAgentChatService(mockHttp.ToHttpClient(), logger);
    
    // Act
    var result = await service.SendAsync("Test message");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test response", result.Message);
}
```

## Architecture Notes

### Why Temporary Models?
- AgentContracts project created but not yet in solution file
- Temporary models inlined to unblock development
- Will be refactored to use shared AgentContracts once solution integration is complete

### Service Discovery
- Uses Aspire service discovery (`https+http://shopping-agent`)
- Automatic endpoint resolution in local and Azure environments
- No hardcoded URLs

### State Management
- Scoped service per Blazor circuit
- Conversation ID persists across messages
- History sent with each request for context

## Troubleshooting

### Chat service not connecting
- Verify ShoppingAssistantAgent is running
- Check Aspire dashboard for service health
- Inspect browser console for errors

### Messages not appearing
- Check browser console for JavaScript errors
- Verify StateHasChanged is called
- Check render mode is InteractiveServer

### Typing indicator stuck
- Check for unhandled exceptions in SendMessage
- Verify isLoading flag is reset in finally block

## Related Files
- `ShoppingAssistantAgent/Program.cs` - Chat endpoint implementation
- `ShoppingAssistantAgent/Models/ChatModelsTemp.cs` - Temporary backend models
- `eShopAppHost/Program.cs` - Service registration and Aspire orchestration
