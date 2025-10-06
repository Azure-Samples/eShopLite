using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using ShoppingAssistantAgent.Endpoints;
using ShoppingAssistantAgent.Services;
using ShoppingAssistantAgent.Tools;
using System.Diagnostics;

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

// Register ZavaAssistant - Main shopping assistant agent
builder.Services.AddScoped<IAgentOrchestrator, ZavaAssistantOrchestrator>();

builder.Services.AddScoped<IImageAgentOrchestrator, ImageAgentOrchestrator>();

// Register ImageAgent - Image processing agent
builder.AddAIAgent("ImageProcessingAgent", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();

    return new ChatClientAgent(
        chatClient,
        name: key,
        instructions:
            @"You are an AI assistant that can analyze images of outdoor and camping products. 
You can help identify products, assess their condition, suggest similar items, and answer questions about what you see in images.
Be descriptive and helpful in your analysis."
    );
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
app.Logger.LogInformation("ZavaAssistant and ImageAgent are ready to process requests");

app.Run();
