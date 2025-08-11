var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");

var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-5-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";

    // Add Azure AI Foundry project
    var foundry = builder.AddAzureAIFoundry("foundry");

    // Add specific model deployments
    var gpt5mini = foundry.AddDeployment(chatDeploymentName, chatDeploymentName, "2025-08-07", "OpenAI");
    var embeddingsDeployment = foundry.AddDeployment(embeddingsDeploymentName, embeddingsDeploymentName, "2", "OpenAI");

    products.WithReference(appInsights)
        .WithReference(foundry)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName)
        .WithExternalHttpEndpoints();

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
