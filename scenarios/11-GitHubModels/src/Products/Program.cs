using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
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
builder.AddSqlServerDbContext<Context>("productsDb");

// Switch between GitHub Models (local) and Azure OpenAI (production)
var useGitHubModels = builder.Configuration.GetValue<bool>("AI_UseGitHubModels", false);

if (useGitHubModels)
{
    // Local development: GitHub Models via OpenAI-compatible endpoint
    var githubToken = builder.Configuration["GitHubModelsToken"] ?? "";
    var githubEndpoint = builder.Configuration["GitHubModelsEndpoint"] ?? "https://models.inference.ai.azure.com";
    var chatModel = builder.Configuration["GitHubModelsChatModel"] ?? "gpt-4.1-mini";
    var embedModel = builder.Configuration["GitHubModelsEmbeddingsModel"] ?? "text-embedding-3-small";

    if (!string.IsNullOrEmpty(githubToken))
    {
        var client = new OpenAIClient(new ApiKeyCredential(githubToken), new OpenAIClientOptions
        {
            Endpoint = new Uri(githubEndpoint)
        });
        builder.Services.AddChatClient(client.GetChatClient(chatModel).AsIChatClient());
        builder.Services.AddEmbeddingGenerator(client.GetEmbeddingClient(embedModel).AsIEmbeddingGenerator());
    }
}
else
{
    // Production: Azure OpenAI via explicit parameters wired from AppHost
    var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
    var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
    var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-4.1-mini";
    var embeddingsDeploymentName = builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-ada-002";

    if (!string.IsNullOrEmpty(endpoint))
    {
        // Use ApiKeyCredential when a key is present; fall back to DefaultAzureCredential for managed identity
        AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
            ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
            : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());
        builder.Services.AddEmbeddingGenerator(
            aoaiClient.GetEmbeddingClient(embeddingsDeploymentName).AsIEmbeddingGenerator());
    }
}

builder.Services.AddSingleton<IConfiguration>(sp => builder.Configuration);

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    return new MemoryContext(
        logger,
        sp.GetService<IChatClient>(),
        sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.MapProductEndpoints();
app.UseStaticFiles();

app.Logger.LogInformation("GitHub Models mode: {UseGitHubModels}", useGitHubModels);
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
    DbInitializer.Initialize(context, useGitHubModels);

    app.Logger.LogInformation("Start fill products in vector db");
    var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    await memoryContext.InitMemoryContextAsync(context);
    app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();