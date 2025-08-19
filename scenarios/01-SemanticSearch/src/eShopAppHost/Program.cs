using Azure.Provisioning.CognitiveServices;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");

IResourceBuilder<IResourceWithConnectionString>? openai;
var chatDeploymentName = "gpt-5-mini";
var embeddingsDeploymentName = "text-embedding-ada-002";

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt5mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-5-mini",
            modelVersion: "2025-08-07");    
    gpt5mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "1");

    products.WithReference(appInsights);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();

    openai = aoai;
}
else
{
    openai = builder.AddConnectionString("openai");
}

products.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

builder.Build().Run();
