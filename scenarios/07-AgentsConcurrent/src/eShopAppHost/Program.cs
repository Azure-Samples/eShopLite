using Azure.Provisioning.CognitiveServices;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var productsDb = sql.AddDatabase("productsdb");
var sqlInsights = sql.AddDatabase("insightsdb");

// Four explicit Aspire parameters for Azure OpenAI (replaces opaque connection string).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                       – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                         – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName                 – chat deployment, e.g. gpt-4.1-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName       – embeddings deployment, e.g. text-embedding-ada-002
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

var insights = builder.AddProject<Projects.Insights>("insights")
    .WithReference(sqlInsights)
    .WaitFor(sqlInsights)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithReference(insights)
    .WaitFor(insights)
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

    aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "2");

    products.WithReference(appInsights);
    insights.WithReference(appInsights);
    store.WithReference(appInsights).WithExternalHttpEndpoints();
}

builder.Build().Run();

