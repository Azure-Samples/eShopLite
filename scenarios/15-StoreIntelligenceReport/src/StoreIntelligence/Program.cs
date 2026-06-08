// StoreIntelligence — standalone Blazor Server app that owns the intelligence
// API (POST/GET signals, GET report) and the dashboard UI. It is registered as
// an externally-accessible Aspire resource ("intelligence") so the presenter can
// browse directly to it from the Aspire dashboard.

using Azure.AI.OpenAI;
using Azure.Identity;
using DataEntities;
using Microsoft.Extensions.AI;
using StoreIntelligence.Endpoints;
using StoreIntelligence.Services;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (telemetry, health checks, service discovery).
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// ── Azure OpenAI chat client (optional — fallback to deterministic if absent) ───
// The AppHost injects these via WithEnvironment from its Aspire parameters.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-5-mini";

if (!string.IsNullOrEmpty(endpoint))
{
    // Prefer API key auth; fall back to DefaultAzureCredential (managed identity in Azure).
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());
}

// ── Intelligence services ────────────────────────────────────────────────────
// StoreSignalStore is a singleton in-memory ring buffer — all requests share one store.
builder.Services.AddSingleton<StoreSignalStore>();

// StoreIntelligenceReportService is also singleton (cheap, stateless except for the store).
// IChatClient may be null — the service degrades gracefully to a deterministic summary.
builder.Services.AddSingleton(sp => new StoreIntelligenceReportService(
    sp.GetRequiredService<StoreSignalStore>(),
    sp.GetRequiredService<ILogger<StoreIntelligenceReportService>>(),
    sp.GetService<IChatClient>())); // null is fine — fallback summary will be used

// ── Razor components (Blazor Server) ────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Aspire default health/liveness endpoints.
app.MapDefaultEndpoints();

// HTTP pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ── Intelligence API endpoints (used by the Products signal producer) ────────
app.MapIntelligenceEndpoints();

// ── Blazor Server components ─────────────────────────────────────────────────
app.MapRazorComponents<StoreIntelligence.Components.App>()
    .AddInteractiveServerRenderMode();

app.Logger.LogInformation(
    "StoreIntelligence started. AzureOpenAI endpoint: {Endpoint}, chat deployment: {Chat}",
    string.IsNullOrEmpty(endpoint) ? "(none — fallback mode)" : endpoint,
    chatDeploymentName);

AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

app.Run();
