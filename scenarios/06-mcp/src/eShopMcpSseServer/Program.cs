using Azure.AI.OpenAI;
using Azure.Identity;
using eShopMcpSseServer.Tools;
using McpSample.AspNetCoreSseServer;
using Microsoft.Extensions.AI;
using Services;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add default services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// add product service
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

builder.Services.AddSingleton<OnlineResearcherService>();
builder.Services.AddHttpClient<OnlineResearcherService>(
    static client => client.BaseAddress = new("https+http://onlineresearcher"));

builder.Services.AddSingleton<WeatherService>();
builder.Services.AddHttpClient<WeatherService>(
    static client => client.BaseAddress = new("https+http://weatheragent"));

builder.Services.AddSingleton<ParkInformationService>();
builder.Services.AddHttpClient<ParkInformationService>(
    static client => client.BaseAddress = new("https+http://parkinformationagent"));

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
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploymentName).AsIChatClient());
}

// add MCP server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<OnlineResearch>()
    .WithTools<ParkInformation>()
    .WithTools<Products>()
    .WithTools<WeatherTool>();

var app = builder.Build();

// Initialize default endpoints
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// map endpoints
app.MapGet("/", () => $"eShopLite-MCP Server! {DateTime.Now}");
app.MapMcp();

app.Run();
