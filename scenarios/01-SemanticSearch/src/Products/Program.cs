using Microsoft.Extensions.AI;
using Microsoft.Extensions.Azure;
using OpenAI;
using Products.Endpoints;
using Products.Memory;
using Products.Models;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("productsDb");

// Add Foundry elements
var azureOpenAiClientName = "foundry";
var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-5-mini";
var embeddingsDeploymentName = builder.Configuration["AI_embeddingsDeploymentName"] ?? "text-embedding-ada-002";

var client = builder.AddOpenAIClient(azureOpenAiClientName);
client.AddChatClient(chatDeploymentName);
client.AddEmbeddingGenerator(embeddingsDeploymentName);

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    var chatClient = sp.GetService<IChatClient>();
    var embeddingGenerator = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
    return new MemoryContext(logger, chatClient, embeddingGenerator);
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
app.Logger.LogInformation($"Azure AI resources\n >> Client Name: {azureOpenAiClientName}\n >> Chat Model: {chatDeploymentName}\n >> Embeddings Model: {embeddingsDeploymentName}");
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

    // Warm-up: validate embedding deployment exists and is reachable
    try
    {
        var eg = app.Services.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
        if (eg is null)
        {
            app.Logger.LogWarning("IEmbeddingGenerator is not registered. Verify AddEmbeddingGenerator and model configuration.");
        }
        else
        {
            await eg.GenerateVectorAsync("warmup");
            app.Logger.LogInformation("Embedding warm-up succeeded using model: {Model}", embeddingsDeploymentName);
        }
    }
    catch (System.ClientModel.ClientResultException crex)
    {
        app.Logger.LogError(crex, "Embedding model not found (HTTP 404). Configure AI_embeddingsDeploymentName to a deployed model (e.g., 'text-embedding-ada-002').");
        app.Logger.LogError("Current model setting: {Model}. Ensure a matching deployment exists in your Azure AI Foundry/OpenAI project named '{ClientName}'.", embeddingsDeploymentName, azureOpenAiClientName);
        // continue to allow app startup; MemoryContext will handle missing vectors gracefully
    }
    catch (Exception egex)
    {
        app.Logger.LogError(egex, "Embedding warm-up failed: {Message}", egex.Message);
    }

    // fill products in vector db
    app.Logger.LogInformation("Start fill products in vector db");
    var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    await memoryContext.InitMemoryContextAsync(context);
    app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();