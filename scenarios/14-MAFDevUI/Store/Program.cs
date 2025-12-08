using AgentServices;
using AgentServices.Checkout;
using AgentServices.Stock.Tools;
using Azure.Identity;
using Microsoft.Agents.AI.DevUI;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.AI;
using Store.Components;
using Store.Services;

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

builder.Services.AddScoped<StockSearchTool>();
builder.Services.AddHttpClient<StockSearchTool>(client =>
{
    client.BaseAddress = new Uri("http+https://products");
});


// Configure Azure OpenAI for agentic checkout
var microsoftFoundryConnectionName = "microsoftfoundry";
var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-5-mini";

var openai = builder.AddAzureOpenAIClient(connectionName: microsoftFoundryConnectionName,
    configureSettings: settings =>
    {
        if (string.IsNullOrEmpty(settings.Key))
        {
            settings.Credential = new DefaultAzureCredential();
        }
    });
openai.AddChatClient(chatDeploymentName)
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
    c.EnableSensitiveData = builder.Environment.IsDevelopment());

// Add Agent settings and agents
builder.AddAgentSettings();
//builder.AddeShopLiteAIAgents();
builder.AddeShopLiteFoundryAgents();

// Add the orchestrator that uses the registered AIAgents directly
builder.Services.AddScoped<AgentCheckoutOrchestrator>();

// Register services for OpenAI responses and conversations (also required for DevUI)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// DEMO Step 5: Add DevUI for agent debugging and visualization
builder.AddDevUI();

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
