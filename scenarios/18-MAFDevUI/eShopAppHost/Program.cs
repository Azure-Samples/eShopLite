var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");

// Four explicit Aspire parameters for Azure OpenAI (replaces opaque connection string).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                       – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                         – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName                 – chat deployment, e.g. gpt-5-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName       – embeddings deployment, e.g. text-embedding-3-small
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

// Azure AI Foundry project endpoint for Foundry-hosted MAF agents
var microsoftfoundryproject = builder.AddConnectionString("microsoftfoundryproject");

var chatDeploymentName = "gpt-5-mini";
var embeddingsDeploymentName = "text-embedding-3-small";

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
    .WithReference(microsoftfoundryproject)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
    .WithReference(microsoftfoundryproject)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: provision Azure OpenAI and Application Insights via Aspire/azd.
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt5mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-5-mini",
            modelVersion: "2025-08-07");
    gpt5mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-3-small",
        modelVersion: "1");
    embeddingsDeployment.Resource.SkuName = "GlobalStandard";

    products.WithReference(appInsights);
    store.WithReference(appInsights);
}

builder.Build().Run();

