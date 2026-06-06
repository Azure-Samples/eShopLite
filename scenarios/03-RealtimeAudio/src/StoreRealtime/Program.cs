using StoreRealtime.Components;
using StoreRealtime.Services;
using StoreRealtime.ContextManagers;
using Azure.AI.OpenAI;
using System.ClientModel;
using OpenAI.RealtimeConversation;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

// Read explicit Azure OpenAI parameters wired from AppHost.
// Parameters:AzureOpenAIEndpoint, Parameters:AzureOpenAIApiKey, Parameters:AzureOpenAIRealtimeDeploymentName
// are set as user-secrets in the eShopAppHost project.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var realtimeDeploymentName = builder.Configuration["AzureOpenAIRealtimeDeploymentName"] ?? "gpt-4o-mini-realtime-preview";

// Build client: use ApiKeyCredential when a key is present; fall back to DefaultAzureCredential
// for managed-identity scenarios (publish/azd mode).
AzureOpenAIClient? aoaiClient = null;
if (!string.IsNullOrEmpty(endpoint))
{
    aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
}

builder.Services.AddSingleton(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Realtime Chat client configuration, modelId: {realtimeDeploymentName}");
    RealtimeConversationClient? realtimeConversationClient = null;
    try
    {
        if (aoaiClient is null)
        {
            logger.LogWarning("AzureOpenAIClient is not configured. RealtimeConversationClient will be null.");
            return realtimeConversationClient!;
        }
        realtimeConversationClient = aoaiClient.GetRealtimeConversationClient(realtimeDeploymentName);
        logger.LogInformation($"Realtime Chat client created, modelId: {realtimeDeploymentName}");
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating realtime conversation client");
    }
    return realtimeConversationClient!;
});

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

builder.Services.AddSingleton(serviceProvider =>
{
    ProductService productService = serviceProvider.GetRequiredService<ProductService>();
    return new ContosoProductContext(productService);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// log values for the AOAI services
app.Logger.LogInformation($@"========================================
Azure OpenAI information
Azure OpenAI Endpoint: {endpoint}
Azure OpenAI Realtime Deployment: {realtimeDeploymentName}
========================================");

app.Run();
