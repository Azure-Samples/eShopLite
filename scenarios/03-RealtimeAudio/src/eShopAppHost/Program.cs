using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

// Five explicit Aspire parameters for Azure OpenAI (no opaque connection string in run mode).
// User-secrets keys (set in eShopAppHost project):
//   Parameters:AzureOpenAIEndpoint                  – e.g. https://<resource>.openai.azure.com/
//   Parameters:AzureOpenAIApiKey                    – API key (stored as a secret)
//   Parameters:AzureOpenAIDeploymentName            – chat deployment, e.g. gpt-4o-mini
//   Parameters:AzureOpenAIEmbeddingsDeploymentName  – embeddings deployment, e.g. text-embedding-ada-002
//   Parameters:AzureOpenAIRealtimeDeploymentName    – realtime deployment, e.g. gpt-4o-mini-realtime-preview
var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");
var aoaiRealtimeDeployment = builder.AddParameter("AzureOpenAIRealtimeDeploymentName");

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

var storeRealtime = builder.AddProject<Projects.StoreRealtime>("realtimestore")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints()
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIRealtimeDeploymentName", aoaiRealtimeDeployment);

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: provision Azure OpenAI and Application Insights via Aspire/azd.
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-4o-mini";
    var realtimeDeploymentName = "gpt-4o-mini-realtime-preview";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    var aoai = builder.AddAzureOpenAI("openai")
        .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
            "gpt-4o-mini",
            "2024-07-18",
            "GlobalStandard",
            10))
        .AddDeployment(new AzureOpenAIDeployment(realtimeDeploymentName,
            "gpt-4o-mini-realtime-preview",
            "2024-12-17",
            "GlobalStandard",
            1))
        .AddDeployment(new AzureOpenAIDeployment(embeddingsDeploymentName,
            "text-embedding-ada-002",
            "2"));

    products.WithReference(appInsights)
        .WithReference(aoai);

    storeRealtime.WithReference(appInsights)
        .WithReference(aoai);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();

