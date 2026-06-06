using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
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

// Azure AI Search – connection string supplied via Aspire (publish mode: WithReference; run mode: user-secrets)
var azureAiSearchName = "azureaisearch";
builder.AddAzureSearchClient(azureAiSearchName);

// Read explicit Azure OpenAI parameters wired from AppHost.
// In run mode these come from the Aspire parameters (user-secrets in eShopAppHost).
// In publish mode they come from the provisioned Azure OpenAI resource via azd.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-4.1-mini";
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
        sp.GetService<SearchIndexClient>());
});

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log Azure OpenAI and Azure AI Search resources
app.Logger.LogInformation($"Azure OpenAI resources\n >> Endpoint: {endpoint}\n >> Chat deployment: {chatDeploymentName}\n >> Embeddings deployment: {embeddingsDeploymentName}");
app.Logger.LogInformation($"Azure AI Search resources\n >> Azure AI Search Name: {azureAiSearchName}");
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

    // init memory context
    var memoryContext = scope.ServiceProvider.GetRequiredService<MemoryContext>();
    try
    {
        memoryContext.InitMemoryContextAsync(context).Wait();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error initializing memory context");
    }
}

app.Run();