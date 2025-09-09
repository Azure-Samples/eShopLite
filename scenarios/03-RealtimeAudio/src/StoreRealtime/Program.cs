using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Realtime;
using StoreRealtime.Components;
using StoreRealtime.ContextManagers;
using StoreRealtime.Services;
using System.ClientModel;
using System.Text.RegularExpressions;
using System.Web;

#pragma warning disable AOAI001 


var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

var azureOpenAiClientName = "openai";

(string endpoint, string apiKey) = GetEndpointAndKey(builder, azureOpenAiClientName);


builder.AddAzureOpenAIClient(azureOpenAiClientName,
    settings =>
    {
        settings.DisableMetrics = false;
        settings.DisableTracing = false;
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            settings.Endpoint = new Uri(endpoint);
        }
    });

// Register RealtimeClient. We prefer using the AzureOpenAIClient helper if available; otherwise build a direct client.
builder.Services.AddSingleton<RealtimeClient>(serviceProvider =>
{
    var chatDeploymentName = builder.Configuration["AI_RealtimeDeploymentName"] ?? "gpt-realtime";
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Configuring RealtimeClient for model {Model}", chatDeploymentName);

    // endpoint = "https://bruno-brk445-resource.cognitiveservices.azure.com/"; // openai/realtime?api-version=2024-10-01-preview&deployment=gpt-realtime";

    //AzureOpenAIClientOptions? options = new(version: AzureOpenAIClientOptions.ServiceVersion.V2024_10_01_Preview)
    //;
    //options.DefaultQueryParameters.Add("deployment", chatDeploymentName);

    //AzureOpenAIClient azureClient = new(new Uri(endpoint), new DefaultAzureCredential(), options);
    //if (!string.IsNullOrEmpty(apiKey))
    //{
    //    azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey), options);
    //}

    AzureOpenAIClient aoaiClient = new(new Uri(endpoint), new ApiKeyCredential(apiKey));
    var c = aoaiClient.GetRealtimeClient();
    return c;

});

builder.Services.AddSingleton<IConfiguration>(sp => builder.Configuration);

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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// log values for the AOAI services
app.Logger.LogInformation(@"========================================
Azure OpenAI information
Azure OpenAI Connection String: {aoaiCnnString}
Azure OpenAI Endpoint: {aoaiEndpoint}
========================================");

app.Run();


static (string endpoint, string apiKey) GetEndpointAndKey(WebApplicationBuilder builder, string name)
{
    var connectionString = builder.Configuration.GetConnectionString(name);
    var parameters = HttpUtility.ParseQueryString(connectionString.Replace(";", "&"));
    return (parameters["Endpoint"], parameters["Key"]);
}

static string? ExtractApiKeyFromConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return null;
    var match = Regex.Match(connectionString, @"Key=([^;\s]+)");
    return match.Success ? match.Groups[1].Value : null;
}