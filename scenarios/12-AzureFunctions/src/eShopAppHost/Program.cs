using Azure.Provisioning.CognitiveServices;

var builder = DistributedApplication.CreateBuilder(args);

// images from https://hub.docker.com/r/microsoft/mssql-server/

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");

// Four explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                      – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                        – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName                – chat deployment, e.g. gpt-5-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName      – embeddings deployment, e.g. text-embedding-3-small
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

var chatDeploymentName = "gpt-5-mini";
var embeddingsDeploymentName = "text-embedding-3-small";

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
    .WithExternalHttpEndpoints();

var semanticSearchFunction = builder.AddAzureFunctionsProject<Projects.SemanticSearchFunction>("semanticsearchfunction")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithReference(semanticSearchFunction)
    .WaitFor(semanticSearchFunction)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt5mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-5-mini",
            modelVersion: "2025-08-07");
    gpt5mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeploymentRes = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: embeddingsDeploymentName,
        modelVersion: "1");
    embeddingsDeploymentRes.Resource.SkuName = "GlobalStandard";

    products.WithReference(appInsights);
    semanticSearchFunction.WithReference(appInsights);
    store.WithReference(appInsights).WithExternalHttpEndpoints();
}

builder.Build().Run();
