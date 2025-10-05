var builder = DistributedApplication.CreateBuilder(args);

// add openai service and models
var chatDeploymentName = "gpt-4o-mini";
var embeddingsDeploymentName = "text-embedding-ada-002";
IResourceBuilder<IResourceWithConnectionString>? openai;

// Add SQL Database
var sqldb = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var productsDb = sqldb
    .WithDataVolume()
    .AddDatabase("productsDb");

// Add Shopping Assistant Agent service
var shoppingAgent = builder.AddProject<Projects.ShoppingAssistantAgent>("shopping-agent");

// Add Products API
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(shoppingAgent)
    .WithReference(productsDb)
    .WaitFor(productsDb);

// Add Store (Frontend)
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithReference(shoppingAgent)
    .WithExternalHttpEndpoints();

// Configure Azure resources for production
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    
    var aoai = builder.AddAzureOpenAI("openai");

    var gpt4omini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-4o-mini",
            modelVersion: "2024-07-18");
    gpt4omini.Resource.SkuCapacity = 10;
    gpt4omini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "2");

    products.WithReference(appInsights)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName)
        .WithExternalHttpEndpoints();

    shoppingAgent.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("OpenAI:DeploymentName", chatDeploymentName)
        .WithExternalHttpEndpoints();

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
    openai = aoai;
}
else
{
    openai = builder.AddConnectionString("openai");
}

products.WithReference(openai);
shoppingAgent.WithReference(openai)
    .WithEnvironment("OpenAI:DeploymentName", chatDeploymentName);

builder.Build().Run();
