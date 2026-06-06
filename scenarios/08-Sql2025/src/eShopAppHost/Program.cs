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
//   Parameters:AzureOpenAIEndpoint                    – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                      – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName              – chat deployment, e.g. gpt-41-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName    – embeddings deployment, e.g. text-embedding-3-small
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: provision Azure OpenAI and Application Insights via Aspire/azd.
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var chatDeploymentName = "gpt-41-mini";
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: chatDeploymentName,
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeploymentName = "text-embedding-3-small";
    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: embeddingsDeploymentName,
        modelVersion: "1");
    embeddingsDeployment.Resource.SkuName = "GlobalStandard";

    products.WithReference(appInsights);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
