using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Products.Endpoints;
using Products.Models;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Context:
// Aspire standard add DbContext service, does not support configuration for vector search
// builder.AddSqlServerDbContext<Context>("productsDb");

// Workaround:
// Get the connection string from configuration, init DbContext and enable vector search
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));

// Read explicit Azure OpenAI parameters wired from AppHost.
// In run mode these come from the Aspire parameters (user-secrets in eShopAppHost).
// In publish mode they come from the provisioned Azure OpenAI resource via azd.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-41-mini";
var embeddingsDeploymentName = builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-3-small";

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

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log Azure OpenAI resources
app.Logger.LogInformation("Azure OpenAI resources\n >> Endpoint: {endpoint}\n >> Chat deployment: {chatDeploymentName}\n >> Embeddings deployment: {embeddingsDeploymentName}",
    endpoint, chatDeploymentName, embeddingsDeploymentName);
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
    var embeddingGenerator = app.Services.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
    if (embeddingGenerator is not null)
    {
        await DbInitializer.Initialize(context, embeddingGenerator);
    }
}

app.Run();
