using AgentServices;
using AgentServices.Checkout;
using AgentServices.Stock.Tools;
using Azure.Identity;
using Microsoft.Agents.AI.DevUI;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.AI;
using OpenAI;
using Store.Components;
using Store.Services;
using System.ClientModel;
using System.ClientModel.Primitives;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// extra console logging to help diagnose startup issues
builder.Logging.AddConsole();

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddSingleton<CustomerService>();

// Register protected session storage used by CartService
builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("http+https://products"));

builder.Services.AddHttpClient<StockSearchTool>(client =>
{
    client.BaseAddress = new Uri("http+https://products");
});


// Configure Azure OpenAI for agentic checkout
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-5-mini";

if (!string.IsNullOrEmpty(endpoint))
{
    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var modelEndpoint) ||
        modelEndpoint.Scheme != Uri.UriSchemeHttps ||
        !string.IsNullOrEmpty(modelEndpoint.UserInfo) ||
        !string.IsNullOrEmpty(modelEndpoint.Query) ||
        !string.IsNullOrEmpty(modelEndpoint.Fragment) ||
        modelEndpoint.AbsolutePath.TrimEnd('/') is not ("" or "/openai/v1"))
    {
        throw new InvalidOperationException("AzureOpenAIEndpoint must be an HTTPS inference endpoint, not a Foundry project URL.");
    }

    var clientOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri(modelEndpoint.GetLeftPart(UriPartial.Authority) + "/openai/v1/")
    };
#pragma warning disable OPENAI001 // Responses and Entra authentication support reasoning models with function tools.
    OpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new OpenAIClient(new BearerTokenPolicy(new DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default"), clientOptions)
        : new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(aoaiClient.GetResponsesClient().AsIChatClient(chatDeploymentName))
        .UseFunctionInvocation()
        .UseOpenTelemetry(configure: c =>
            c.EnableSensitiveData = builder.Environment.IsDevelopment());
#pragma warning restore OPENAI001
}

// Add Agent settings and agents
builder.AddAgentSettings();
if (builder.Configuration.GetValue<bool>("AgentSettings:UseFoundryAgents"))
{
    builder.AddeShopLiteFoundryAgents();
}
else
{
    if (string.IsNullOrWhiteSpace(endpoint))
    {
        throw new InvalidOperationException("Local agent mode requires AzureOpenAIEndpoint.");
    }

    builder.AddeShopLiteAIAgents();
}

// Add the orchestrator that uses the registered AIAgents directly
builder.Services.AddScoped<AgentCheckoutOrchestrator>();

// Register services for OpenAI responses and conversations (also required for DevUI)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// DEMO Step 5: Add DevUI for agent debugging and visualization
if (builder.Environment.IsDevelopment())
{
    builder.AddDevUI();
}

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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (builder.Environment.IsDevelopment())
{
    // DEMO Step 5: Map DevUI and related endpoints for agent debugging
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();

    // Map DevUI endpoint to /devui
    app.MapDevUI();
}

app.Logger.LogInformation("Starting Store app...");
app.Run();

public partial class Program { }
