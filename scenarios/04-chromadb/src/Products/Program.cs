using Azure.AI.OpenAI;
using Azure.Identity;
using ChromaDB.Client;
using Microsoft.Extensions.AI;
using Products.Endpoints;
using Products.Memory;
using Products.Models;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

// Read explicit Azure OpenAI parameters wired from AppHost.
// In run mode these come from Aspire parameters (user-secrets in eShopAppHost).
// In publish mode they come from the provisioned Azure OpenAI resource via azd.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-41-mini";
var embeddingsDeploymentName = builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-ada-002";

if (!string.IsNullOrEmpty(endpoint))
{
    // Build client: use ApiKeyCredential when a key is present; fall back to DefaultAzureCredential
    // for managed-identity scenarios (publish/azd mode).
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());
    builder.Services.AddEmbeddingGenerator(
        aoaiClient.GetEmbeddingClient(embeddingsDeploymentName).AsIEmbeddingGenerator());
}

// get the ChromaDB Collection Client
builder.Services.AddSingleton<ChromaCollectionClient>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    var config = serviceProvider.GetService<IConfiguration>()!;
    ChromaCollectionClient chromaCollectionClient = null;
    try
    {
        // get chromaDB service Uri from configuration
        var chromaDbService = config.GetSection("services:chroma:chromaendpoint:0");
        var chromaDbUri = chromaDbService.Value;
        logger.LogInformation($"ChromaDB client configuration, key: {chromaDbService.Key}");
        logger.LogInformation($"ChromaDB client configuration, value: {chromaDbService.Value}");

        if (!string.IsNullOrEmpty(chromaDbUri) && !chromaDbUri.EndsWith("/api/v1"))
        {
            logger.LogInformation("ChromaDB connection string does not end with /api/v1, adding it");
            chromaDbUri += "/api/v1";
        }
        logger.LogInformation($"ChromaDB client uri: {chromaDbUri}");

        var configOptions = new ChromaConfigurationOptions(uri: chromaDbUri);
        var httpChromaClient = new HttpClient();
        var chromaClient = new ChromaClient(configOptions, httpChromaClient);

        var collection = chromaClient.GetOrCreateCollection("products").GetAwaiter().GetResult();
        chromaCollectionClient = new ChromaCollectionClient(collection, configOptions, httpChromaClient);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating chromaDB client");
    }
    return chromaCollectionClient;
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
    return new MemoryContext(
        logger,
        sp.GetService<IChatClient>(),
        sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>(),
        sp.GetService<ChromaCollectionClient>());
});

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log Azure OpenAI resources
app.Logger.LogInformation($"Azure OpenAI resources\n >> Endpoint: {endpoint}\n >> Chat deployment: {chatDeploymentName}\n >> Embeddings deployment: {embeddingsDeploymentName}");
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

// get from builder.Configuration the service named chromadb
var chromaDbService = builder.Configuration.GetSection("services:chroma:chromaendpoint:0");
app.Logger.LogInformation($"ChromaDB client configuration, key: {chromaDbService.Key}");
app.Logger.LogInformation($"ChromaDB client configuration, value: {chromaDbService.Value}");

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

    // init memory context
    var memoryContext = scope.ServiceProvider.GetRequiredService<MemoryContext>();
    try
    {
        app.Logger.LogInformation("Initializing memory context");
        var result = memoryContext.InitMemoryContextAsync(context).Result;
        app.Logger.LogInformation($"Memory context initialized: {result}");
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error initializing memory context");
    }
}

app.Run();

