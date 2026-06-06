using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("sqldb");

// Four explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                    – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                      – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName              – chat deployment, e.g. gpt-4.1-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName    – embeddings deployment, e.g. text-embedding-ada-002
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

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
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

    var azureaisearch = builder.AddAzureSearch("azureaisearch");
    azureaisearch.WithReferenceRelationship(aoai)
        .WithReferenceRelationship(appInsights)
        .WithReferenceRelationship(sqldb);

    products.WithReference(azureaisearch)
            .WithReference(appInsights);

    store.WithReference(azureaisearch)
        .WithReference(appInsights)
        .WithExternalHttpEndpoints();
}
builder.Build().Run();
