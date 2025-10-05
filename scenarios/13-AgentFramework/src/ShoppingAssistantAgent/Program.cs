using ShoppingAssistantAgent.Tools;
using ShoppingAssistantAgent.Models;
using OpenAI;
using Microsoft.Extensions.AI;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP client for Products API
builder.Services.AddHttpClient("products", client =>
{
    // Will be configured via Aspire service discovery
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Agent Tools
builder.Services.AddSingleton<SearchCatalogTool>();
builder.Services.AddSingleton<ProductDetailsTool>();
builder.Services.AddSingleton<AddToCartTool>();

// Configure OpenAI client with gpt-4.1-mini
var azureOpenAiClientName = "openai";
var chatDeploymentName = builder.Configuration["OpenAI:DeploymentName"] ?? "gpt-4.1-mini";
builder.AddAzureOpenAIClient(azureOpenAiClientName);

// Create IChatClient with function calling support - Microsoft Agent Framework pattern
builder.Services.AddSingleton<IChatClient>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Configuring chat client with model: {ChatDeploymentName}", chatDeploymentName);
    
    try
    {
        var openAIClient = serviceProvider.GetRequiredService<OpenAIClient>();
        var chatClient = openAIClient.GetChatClient(chatDeploymentName);
        
        // Get tool instances for function calling
        var searchTool = serviceProvider.GetRequiredService<SearchCatalogTool>();
        var detailsTool = serviceProvider.GetRequiredService<ProductDetailsTool>();
        var cartTool = serviceProvider.GetRequiredService<AddToCartTool>();
        
        // Create AI functions from tools
        var tools = new List<AIFunction>
        {
            AIFunctionFactory.Create(searchTool.SearchProductsAsync),
            AIFunctionFactory.Create(detailsTool.GetProductDetailsAsync),
            AIFunctionFactory.Create(cartTool.AddProductToCartAsync)
        };
        
        // Build chat client with function invocation enabled (Agent Framework pattern)
        // Using Microsoft.Extensions.AI pattern similar to MCP scenario
        return chatClient
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating chat client");
        throw;
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Chat endpoint using Microsoft Agent Framework pattern with function calling
app.MapPost("/api/agent/chat", async (
    ChatRequest request,
    IChatClient chatClient,
    SearchCatalogTool searchTool,
    ProductDetailsTool detailsTool,
    AddToCartTool cartTool,
    ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Received chat message: {Message}", request.Message);

        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
        
        // Build conversation history using Microsoft.Extensions.AI
        var messages = new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, @"You are a helpful shopping assistant for an outdoor camping products store. 
You can help customers:
- Search for products by name or description using the SearchProductsAsync function
- Get detailed information about specific products using the GetProductDetailsAsync function
- Add products to their shopping cart using the AddProductToCartAsync function

Be friendly, concise, and helpful. When customers ask about products, use the available tools to search the catalog.
If they want product details, ask for the product ID if not provided.
When adding to cart, confirm the action with the customer.")
        };

        // Add conversation history if provided
        if (request.History?.Any() == true)
        {
            foreach (var msg in request.History.TakeLast(10)) // Keep last 10 messages for context
            {
                var role = msg.Role.ToLower() == "user" ? ChatRole.User : ChatRole.Assistant;
                messages.Add(new Microsoft.Extensions.AI.ChatMessage(role, msg.Content));
            }
        }

        // Add current user message
        messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, request.Message));

        // Create AI functions from tools for this request
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(searchTool.SearchProductsAsync),
            AIFunctionFactory.Create(detailsTool.GetProductDetailsAsync),
            AIFunctionFactory.Create(cartTool.AddProductToCartAsync)
        };

        // Call AI with automatic function calling (Agent Framework pattern)
        var chatOptions = new ChatOptions
        {
            Temperature = 0.7f,
            MaxOutputTokens = 800,
            Tools = tools
        };

        logger.LogInformation("Calling AI agent with {MessageCount} messages and {ToolCount} tools", messages.Count, tools.Count);
        
        // The IChatClient with function invocation will automatically invoke tools as needed
        // Using GetResponseAsync from Microsoft.Extensions.AI (returns full ChatResponse with messages)
        var chatResult = await chatClient.GetResponseAsync(messages, chatOptions);

        var assistantMessage = chatResult?.Text ?? "I'm sorry, I couldn't process that request.";
        
        logger.LogInformation("Agent response generated: {Response}", 
            assistantMessage.Substring(0, Math.Min(100, assistantMessage.Length)));

        var response = new ShoppingAssistantAgent.Models.ChatResponse
        {
            Message = assistantMessage,
            ConversationId = conversationId
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing chat request");
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("AgentChat")
.WithOpenApi();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
