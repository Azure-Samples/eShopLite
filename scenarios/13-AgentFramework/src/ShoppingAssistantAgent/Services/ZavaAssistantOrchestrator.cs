using Microsoft.Extensions.AI;
using ShoppingAssistantAgent.Models;
using ShoppingAssistantAgent.Tools;
using System.Diagnostics;
using AIMessage = Microsoft.Extensions.AI.ChatMessage;
using AIResponse = Microsoft.Extensions.AI.ChatResponse;

namespace ShoppingAssistantAgent.Services;

/// <summary>
/// ZavaAssistant - Main shopping assistant agent using Microsoft Agent Framework pattern
/// </summary>
public class ZavaAssistantOrchestrator : IAgentOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly SearchCatalogTool _searchTool;
    private readonly ProductDetailsTool _detailsTool;
    private readonly AddToCartTool _cartTool;
    private readonly ILogger<ZavaAssistantOrchestrator> _logger;
    private static readonly ActivitySource ActivitySource = new("ShoppingAssistant.ZavaAgent");

    public ZavaAssistantOrchestrator(
        IChatClient chatClient,
        SearchCatalogTool searchTool,
        ProductDetailsTool detailsTool,
        AddToCartTool cartTool,
        ILogger<ZavaAssistantOrchestrator> logger)
    {
        _chatClient = chatClient;
        _searchTool = searchTool;
        _detailsTool = detailsTool;
        _cartTool = cartTool;
        _logger = logger;
    }

    public async Task<Models.ChatResponse> ProcessMessageAsync(
        string message,
        string conversationId,
        List<Models.ChatMessage> history)
    {
        using var activity = ActivitySource.StartActivity("ProcessMessage");
        activity?.SetTag("conversation.id", conversationId);
        activity?.SetTag("message.length", message.Length);

        try
        {
            _logger.LogInformation(
                "ZavaAssistant processing message for conversation {ConversationId}", 
                conversationId);

            // Build conversation messages using Microsoft.Extensions.AI
            var messages = new List<AIMessage>
            {
                new(ChatRole.System, @"You are Zava, a helpful shopping assistant for an outdoor camping products store. 
You can help customers:
- Search for products by name or description using the SearchProductsAsync function
- Get detailed information about specific products using the GetProductDetailsAsync function
- Add products to their shopping cart using the AddProductToCartAsync function

Be friendly, concise, and helpful. When customers ask about products, use the available tools to search the catalog.
If they want product details, ask for the product ID if not provided.
When adding to cart, confirm the action with the customer.
Always be enthusiastic about outdoor adventures and camping!")
            };

            // Add conversation history (last 10 messages for context)
            if (history?.Any() == true)
            {
                foreach (var msg in history.TakeLast(10))
                {
                    var role = msg.Role.ToLower() == "user" ? ChatRole.User : ChatRole.Assistant;
                    messages.Add(new AIMessage(role, msg.Content));
                }
            }

            // Add current user message
            messages.Add(new AIMessage(ChatRole.User, message));

            // Create AI functions from tools - Microsoft Agent Framework pattern
            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_searchTool.SearchProductsAsync),
                AIFunctionFactory.Create(_detailsTool.GetProductDetailsAsync),
                AIFunctionFactory.Create(_cartTool.AddProductToCartAsync)
            };

            // Configure chat options with function calling
            var chatOptions = new ChatOptions
            {
                Temperature = 0.7f,
                MaxOutputTokens = 800,
                Tools = tools
            };

            _logger.LogInformation(
                "Calling ZavaAssistant with {MessageCount} messages and {ToolCount} tools", 
                messages.Count, 
                tools.Count);

            // Call AI with automatic function calling - Agent Framework pattern
            var chatCompletion = await _chatClient.CompleteAsync(messages, chatOptions);

            var assistantMessage = chatCompletion?.Message?.Text 
                ?? "I'm sorry, I couldn't process that request.";

            activity?.SetTag("response.length", assistantMessage.Length);
            activity?.SetTag("response.success", true);

            _logger.LogInformation(
                "ZavaAssistant response generated: {ResponsePreview}",
                assistantMessage.Substring(0, Math.Min(100, assistantMessage.Length)));

            return new Models.ChatResponse
            {
                Message = assistantMessage,
                ConversationId = conversationId
            };
        }
        catch (Exception ex)
        {
            activity?.SetTag("response.success", false);
            activity?.SetTag("error.type", ex.GetType().Name);
            
            _logger.LogError(ex, 
                "Error in ZavaAssistant processing message for conversation {ConversationId}", 
                conversationId);
            
            throw;
        }
    }
}
