using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using ShoppingAssistantAgent.Endpoints;
using ShoppingAssistantAgent.Services;
using ShoppingAssistantAgent.Tools;
using System.Diagnostics;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults with enhanced telemetry
builder.AddServiceDefaults();

// Configure OpenTelemetry for agents
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("ShoppingAssistant.ZavaAgent");
        tracing.AddSource("ShoppingAssistant.ImageAgent");
        tracing.AddSource("ShoppingAssistant.Telemetry");
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("ShoppingAssistant.Metrics");
    });

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Agent Tools
builder.Services.AddScoped<SearchCatalogTool>();
builder.Services.AddHttpClient<SearchCatalogTool>(
    static client => client.BaseAddress = new("https+http://products"));

builder.Services.AddSingleton<ProductDetailsTool>();
builder.Services.AddHttpClient<ProductDetailsTool>(
    static client => client.BaseAddress = new("https+http://products"));

builder.Services.AddSingleton<AddToCartTool>();
builder.Services.AddHttpClient<AddToCartTool>(
    static client => client.BaseAddress = new("https+http://products"));

// Register Telemetry Agent for monitoring and logging
builder.Services.AddSingleton<TelemetryAgent>();

// Configure OpenAI client with gpt-4o-mini
var azureOpenAiClientName = "openai";
var chatDeploymentName = builder.Configuration["OpenAI:DeploymentName"] ?? "gpt-4o-mini";
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
        
        // Build chat client with function invocation enabled (Agent Framework pattern)
        // Using Microsoft.Extensions.AI pattern with IChatClient adapter
        var aiChatClient = chatClient.AsIChatClient();
        
        return new ChatClientBuilder(aiChatClient)
            .UseFunctionInvocation()
            .Build();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating chat client");
        throw;
    }
});

// Register ZavaAssistant - Main shopping assistant agent using ChatClientAgent pattern
builder.AddAIAgent("ZavaAssistant", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var searchTool = sp.GetRequiredService<SearchCatalogTool>();
    var detailsTool = sp.GetRequiredService<ProductDetailsTool>();
    var cartTool = sp.GetRequiredService<AddToCartTool>();

    return new ChatClientAgent(
        chatClient,
        new ChatClientAgentOptions
        {
            Name = key,
            Instructions = @"You are Zava, a helpful shopping assistant for an outdoor camping products store. 
You can help customers:
- Search for products by name or description using the SearchProductsAsync function
- Get detailed information about specific products using the GetProductDetailsAsync function
- Add products to their shopping cart using the AddProductToCartAsync function

Be friendly, concise, and helpful. When customers ask about products, use the available tools to search the catalog.
If they want product details, ask for the product ID if not provided.
When adding to cart, confirm the action with the customer.
Always be enthusiastic about outdoor adventures and camping!",
            ChatOptions = new ChatOptions
            {
                Temperature = 0.7f,
                MaxOutputTokens = 800,
                Tools = [
                    AIFunctionFactory.Create(searchTool.SearchProductsAsync),
                    AIFunctionFactory.Create(detailsTool.GetProductDetailsAsync),
                    AIFunctionFactory.Create(cartTool.AddProductToCartAsync)
                ],
            }
        });
});

// Register Telemetry and Logging Agent
builder.AddAIAgent("TelemetryAgent", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var telemetryAgent = sp.GetRequiredService<TelemetryAgent>();

    return new ChatClientAgent(
        chatClient,
        new ChatClientAgentOptions
        {
            Name = key,
            Instructions = @"You are a telemetry and logging assistant that monitors conversations and tracks metrics.
You observe user interactions and provide insights about usage patterns, popular queries, and system health.
You help identify issues and suggest improvements based on conversation analysis.",
            ChatOptions = new ChatOptions
            {
                Temperature = 0.3f,
                MaxOutputTokens = 500
            }
        });
});

// Register ImageAgent - Image processing agent using ChatClientAgent pattern
builder.AddAIAgent("ImageAgent", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();

    return new ChatClientAgent(
        chatClient,
        new ChatClientAgentOptions
        {
            Name = key,
            Instructions = @"You are an AI assistant that can analyze images of outdoor and camping products. 
You can help identify products, assess their condition, suggest similar items, and answer questions about what you see in images.
Be descriptive and helpful in your analysis. When you see product images, describe what you observe and suggest relevant products from our catalog.",
            ChatOptions = new ChatOptions
            {
                Temperature = 0.7f,
                MaxOutputTokens = 1000
            }
        });
}); 

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map agent endpoints from external class
app.MapAgentEndpoints();

app.Logger.LogInformation("Shopping Assistant Agent with Microsoft Agent Framework started successfully");
app.Logger.LogInformation("ZavaAssistant, ImageAgent, and TelemetryAgent are ready to process requests");

app.Run();
