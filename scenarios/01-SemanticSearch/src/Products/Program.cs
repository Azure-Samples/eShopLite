using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Embeddings;
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


var azureOpenAIConnectionName = "openai";
var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-5-mini";
var embeddingsDeploymentName = builder.Configuration["AI_embeddingsDeploymentName"] ?? "text-embedding-ada-002";

builder.AddAzureOpenAIClient(connectionName: azureOpenAIConnectionName,
    configureSettings: settings =>
    {
        if (string.IsNullOrEmpty(settings.Key))
        {
            settings.Credential = new DefaultAzureCredential();
        }
    }).AddChatClient(chatDeploymentName);

builder.AddAzureOpenAIClient(azureOpenAIConnectionName,
    configureSettings: settings =>
    {
        if (string.IsNullOrEmpty(settings.Key))
        {
            settings.Credential = new DefaultAzureCredential();
        }
    }).AddEmbeddingGenerator(embeddingsDeploymentName);

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    return new MemoryContext(logger, sp.GetService<IChatClient>(), sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
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
app.Logger.LogInformation($"Azure OpenAI resources\n >> OpenAI Client Name: {azureOpenAIConnectionName}");
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