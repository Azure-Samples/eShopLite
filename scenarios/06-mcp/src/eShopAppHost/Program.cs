var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("sqldb");

// Four explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                     – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                       – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName               – chat deployment, e.g. gpt-4.1-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName     – embeddings deployment, e.g. text-embedding-ada-002
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);

var onlineresearcher = builder.AddProject<Projects.OnlineResearcher>("onlineresearcher")
    .WithExternalHttpEndpoints();

var parkinformationagent = builder.AddProject<Projects.ParkInformationAgent>("parkinformationagent")
    .WithExternalHttpEndpoints();

var weatheragent = builder.AddProject<Projects.WeatherAgent>("weatheragent")
    .WithExternalHttpEndpoints();

var eshopmcpserver = builder.AddProject<Projects.eShopMcpSseServer>("eshopmcpserver")
    .WithReference(onlineresearcher)
    .WaitFor(onlineresearcher)
    .WithReference(products)
    .WaitFor(products)
    .WithReference(weatheragent)
    .WaitFor(weatheragent)
    .WithReference(parkinformationagent)
    .WaitFor(parkinformationagent)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithExternalHttpEndpoints();


var store = builder.AddProject<Projects.Store>("store")
    .WithReference(eshopmcpserver)
    .WaitFor(eshopmcpserver)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-41-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    var aoai = builder.AddAzureOpenAI("openai");
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-4.1-mini",
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "2");

    products.WithReference(appInsights);

    eshopmcpserver.WithReference(appInsights);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
