using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var demo = builder.Configuration.GetValue<bool>("Demo");

var products = builder.AddProject<Projects.Products>("products")
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Demo", demo.ToString())
    .WithHttpHealthCheck("/health");
var agent = builder.AddProject<Projects.Agent>("agent")
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(products)
    .WaitFor(products)
    .WithEnvironment("Demo", demo.ToString())
    .WithHttpHealthCheck("/health");

if (!demo)
{
    var endpoint = builder.AddParameter("AzureOpenAIEndpoint");
    var key = builder.AddParameter("AzureOpenAIApiKey", secret: true);
    var chat = builder.AddParameter("AzureOpenAIDeploymentName", "gpt-6-luna");
    var embeddings = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName", "text-embedding-3-small");
    var database = builder.AddSqlServer("sql")
        .WithImageTag("2025-latest")
        .WithDataVolume()
        .AddDatabase("productsDb");
    products.WithReference(database).WaitFor(database)
        .WithEnvironment("AzureOpenAIEndpoint", endpoint)
        .WithEnvironment("AzureOpenAIApiKey", key)
        .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", embeddings);
    agent.WithEnvironment("AzureOpenAIEndpoint", endpoint)
        .WithEnvironment("AzureOpenAIApiKey", key)
        .WithEnvironment("AzureOpenAIDeploymentName", chat);
}

builder.AddProject<Projects.Store>("store")
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(products).WaitFor(products)
    .WithReference(agent).WaitFor(agent)
    .WithEnvironment("Demo", demo.ToString())
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
