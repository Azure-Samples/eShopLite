using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Services;
using Store.Components;
using Store.Services;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// add a named service for a HttpClient object named "productsHttpClient"
builder.Services.AddHttpClient("productsHttpClient", static client => client.BaseAddress = new("https+http://products"));

builder.Services.AddSingleton<McpServerService>();

// Read explicit Azure OpenAI parameters wired from AppHost.
var endpoint = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploymentName = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-4.1-mini";

if (!string.IsNullOrEmpty(endpoint))
{
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    builder.Services.AddChatClient(
        aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build());
}

// create Mcp Client
builder.Services.AddSingleton<IMcpClient>(serviceProvider =>
{
    // get named service for a HttpClient object named "productsHttpClient"
    var h = serviceProvider.GetService<IHttpClientFactory>();
    var httpClient = h.CreateClient("productsHttpClient");

    McpClientOptions mcpClientOptions = new()
    {
        ClientInfo = new() { Name = "AspNetCoreSseClient", Version = "1.0.0" }
    };

    // can't use the service discovery for ["https +http://aspnetsseserver"]
    // fix: read the environment value for the key 'services__aspnetsseserver__https__0' to get the url for the aspnet core sse server
    var serviceName = "eshopmcpserver";
    var name = $"services__{serviceName}__https__0";
    var url = Environment.GetEnvironmentVariable(name) + "/sse";

    SseClientTransportOptions sseClientTransportOptions = new()
    {
        Endpoint = new Uri(url)
    };

    SseClientTransport clientTransport = new(sseClientTransportOptions, httpClient);

    var mcpClient = McpClientFactory.CreateAsync(clientTransport, mcpClientOptions).GetAwaiter().GetResult();
    return mcpClient;
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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();