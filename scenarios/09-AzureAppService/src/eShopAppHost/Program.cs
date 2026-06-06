using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// add support for Azure AppService 
var appsvc = builder.AddAzureAppServiceEnvironment("appsvc");

// Explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// Set via user-secrets in eShopAppHost for local dev:
//   Parameters:AzureOpenAIEndpoint              – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName        – chat deployment, e.g. gpt-4.1-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName – embeddings deployment, e.g. text-embedding-ada-002
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

var products = builder.AddProject<Projects.Products>("products")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: provision Azure OpenAI via Aspire/azd.
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

    store.WithExternalHttpEndpoints();
}

builder.Build().Run();
