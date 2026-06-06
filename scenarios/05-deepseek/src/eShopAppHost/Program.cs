var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume()
            .AddDatabase("sqldb");

// Four explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                   – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                     – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName             – chat deployment, e.g. gpt-41-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName   – embeddings deployment, e.g. text-embedding-ada-002
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

// Three explicit Aspire parameters for DeepSeek-R1 (separate endpoint/key/deployment).
// User-secrets keys:
//   Parameters:DeepSeekEndpoint       – e.g. https://<resource>.eastus.models.ai.azure.com/
//   Parameters:DeepSeekApiKey         – API key (stored as a secret)
//   Parameters:DeepSeekDeploymentName – e.g. DeepSeek-R1
var deepSeekEndpoint = builder.AddParameter("DeepSeekEndpoint");
var deepSeekApiKey = builder.AddParameter("DeepSeekApiKey", secret: true);
var deepSeekDeployment = builder.AddParameter("DeepSeekDeploymentName");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
    .WithEnvironment("DeepSeekEndpoint", deepSeekEndpoint)
    .WithEnvironment("DeepSeekApiKey", deepSeekApiKey)
    .WithEnvironment("DeepSeekDeploymentName", deepSeekDeployment)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: provision Azure OpenAI and Application Insights via Aspire/azd.
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt41mini = aoai.AddDeployment(name: "gpt-41-mini",
            modelName: "gpt-4.1-mini",
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: "text-embedding-ada-002",
        modelName: "text-embedding-ada-002",
        modelVersion: "2");

    products.WithReference(appInsights);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
