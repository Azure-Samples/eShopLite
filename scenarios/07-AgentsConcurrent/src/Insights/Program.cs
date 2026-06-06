using Azure.AI.OpenAI;
using Azure.Identity;
using Insights.Agents;
using Insights.Endpoints;
using Insights.Models;
using Microsoft.Extensions.AI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddOpenApi();

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// Add DbContext service
builder.AddSqlServerDbContext<Context>("insightsdb");

// Read explicit Azure OpenAI parameters wired from AppHost.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-4.1-mini";

if (!string.IsNullOrEmpty(endpoint))
{
    // Build client: use ApiKeyCredential when a key is present; fall back to DefaultAzureCredential
    // for managed-identity scenarios (publish/azd mode).
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());
}

builder.Services.AddAgents();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapInsightsEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// log Azure OpenAI resources
app.Logger.LogInformation("Azure OpenAI resources\n >> Endpoint: {endpoint}\n >> Chat deployment: {chatDeploymentName}",
    endpoint, chatDeploymentName);
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

// manage db
var scope = app.Services.CreateScope();
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

app.Run();
