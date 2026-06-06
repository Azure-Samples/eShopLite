using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
using Products.Endpoints;
using Products.Memory;
using Products.Models;
using System.ClientModel;

// Key names for keyed IChatClient services
const string chatClientNameOpenAI = "chatClientOpenAI";
const string chatClientNameDeepSeekR1 = "chatClientDeepSeekR1";

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// Add default services
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

// Read DeepSeek-R1 parameters (separate endpoint/key/deployment)
var deepseekEndpoint = builder.Configuration["DeepSeekEndpoint"] ?? "";
var deepseekApiKey = builder.Configuration["DeepSeekApiKey"] ?? "";
var deepseekDeploymentName = builder.Configuration["DeepSeekDeploymentName"] ?? "DeepSeek-R1";

if (!string.IsNullOrEmpty(endpoint))
{
    // Build Azure OpenAI client: use ApiKeyCredential when a key is present; fall back to
    // DefaultAzureCredential for managed-identity scenarios (publish/azd mode).
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);

    builder.Services.AddKeyedSingleton<IChatClient>(chatClientNameOpenAI,
        aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());

    builder.Services.AddEmbeddingGenerator(
        aoaiClient.GetEmbeddingClient(embeddingsDeploymentName).AsIEmbeddingGenerator());
}

if (!string.IsNullOrEmpty(deepseekEndpoint))
{
    IChatClient deepseekChatClient;
    if (string.IsNullOrEmpty(deepseekApiKey))
    {
        // No API key – use DefaultAzureCredential (Azure AI Foundry managed identity)
        var client = new AzureOpenAIClient(new Uri(deepseekEndpoint), new DefaultAzureCredential());
        deepseekChatClient = client.GetChatClient(deepseekDeploymentName).AsIChatClient();
    }
    else
    {
        // API key present – use OpenAI-compatible endpoint (Azure AI Foundry serverless)
        var options = new OpenAIClientOptions { Endpoint = new Uri(deepseekEndpoint) };
        var client = new OpenAIClient(new ApiKeyCredential(deepseekApiKey), options);
        deepseekChatClient = client.GetChatClient(deepseekDeploymentName).AsIChatClient();
    }
    builder.Services.AddKeyedSingleton<IChatClient>(chatClientNameDeepSeekR1, deepseekChatClient);
}

builder.Services.AddSingleton<IConfiguration>(_ => builder.Configuration);

// Register MemoryContext
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger!.LogInformation("Creating memory context");
    return new MemoryContext(
        logger,
        sp.GetKeyedService<IChatClient>(chatClientNameOpenAI),
        sp.GetKeyedService<IChatClient>(chatClientNameDeepSeekR1),
        sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.MapProductEndpoints();
app.UseStaticFiles();

app.Logger.LogInformation($"Azure OpenAI resources -> Endpoint: {endpoint}, Chat: {chatDeploymentName}, Embeddings: {embeddingsDeploymentName}");
app.Logger.LogInformation($"DeepSeek-R1 resources -> Endpoint: {deepseekEndpoint}, Deployment: {deepseekDeploymentName}");
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

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
