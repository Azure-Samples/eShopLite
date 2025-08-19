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

IResourceBuilder<IResourceWithConnectionString>? openai;

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithExternalHttpEndpoints();

var semanticSearchFunction = builder.AddAzureFunctionsProject<Projects.SemanticSearchFunction>("semanticsearchfunction")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithReference(semanticSearchFunction)
    .WaitFor(semanticSearchFunction)
    .WithExternalHttpEndpoints();

var chatDeploymentName = "gpt-5-mini";
var embeddingsDeploymentName = "text-embedding-3-small";

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: chatDeploymentName,
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: embeddingsDeploymentName,
        modelVersion: "1");

    products.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

    semanticSearchFunction.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

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

semanticSearchFunction.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

builder.Build().Run();
