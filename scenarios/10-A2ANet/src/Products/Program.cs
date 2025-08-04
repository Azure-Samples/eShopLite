using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Products.Endpoints;
using Products.Memory;
using Products.Models;
using Products.Services;
using Products.Services.Agents;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);
// Register ProductService for DI so it can be injected into A2AOrchestrationService
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri("http://products");
});
builder.Services.AddScoped<IProductService, ProductService>();

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

// in dev scenarios rename this to "openaidev", and check the documentation to reuse existing AOAI resources
var azureOpenAiClientName = "openai";
var chatDeploymentName = "gpt-4.1-mini";
var embeddingsDeploymentName = "text-embedding-ada-002";
builder.AddAzureOpenAIClient(azureOpenAiClientName);

// get azure openai client and create Chat client from aspire hosting configuration
builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Chat client configuration, modelId: {chatDeploymentName}");
    ChatClient chatClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        chatClient = client.GetChatClient(chatDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating chat client");
    }
    return chatClient;
});

// get azure openai client and create embedding client from aspire hosting configuration
builder.Services.AddSingleton<EmbeddingClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Embeddings client configuration, modelId: {embeddingsDeploymentName}");
    EmbeddingClient embeddingsClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        embeddingsClient = client.GetEmbeddingClient(embeddingsDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating embeddings client");
    }
    return embeddingsClient;
});

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    return new MemoryContext(logger, sp.GetService<ChatClient>(), sp.GetService<EmbeddingClient>());
});

// Configure HttpClients for agents
builder.Services.AddHttpClient("InventoryAgent", client =>
{
    client.BaseAddress = new Uri("http://inventory-agent");
});

builder.Services.AddHttpClient("PromotionsAgent", client =>
{
    client.BaseAddress = new Uri("http://promotions-agent");
});

builder.Services.AddHttpClient("ResearcherAgent", client =>
{
    client.BaseAddress = new Uri("http://researcher-agent");
});

// Add A2A Agents using A2A .NET SDK
builder.Services.AddScoped<InventoryAgent>();
builder.Services.AddScoped<PromotionsAgent>();
builder.Services.AddScoped<ResearcherAgent>();

// Add A2A Orchestration Service
builder.Services.AddScoped<IA2AOrchestrationService, A2AOrchestrationService>();

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log Azure OpenAI resources
app.Logger.LogInformation($"Azure OpenAI resources\n >> OpenAI Client Name: {azureOpenAiClientName}");
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

// manage db
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        app.Logger.LogInformation("Ensure database created");
        context.Database.EnsureCreated();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error creating database");
    }
    DbInitializer.Initialize(context);

    app.Logger.LogInformation("Start fill products in vector db");
    var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    await memoryContext.InitMemoryContextAsync(context);
    app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();