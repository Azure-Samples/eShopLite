var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Database
var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

// Add Shopping Assistant Agent service
var shoppingAgent = builder.AddProject<Projects.ShoppingAssistantAgent>("shopping-agent");

// Add Products API
var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WithReference(shoppingAgent)
    .WaitFor(sqldb);

// Add Store (Frontend)
var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WithReference(shoppingAgent)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

// Configure Azure resources for production
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    
    var chatDeploymentName = "gpt-4o-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
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
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

    shoppingAgent.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("OpenAI:DeploymentName", chatDeploymentName);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
