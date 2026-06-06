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
    .WaitFor(productsDb);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // Production: Azure OpenAI via explicit Aspire parameters (no auto-provisioning).
    // Set these in eShopAppHost user-secrets or via azd:
    //   Parameters:AzureOpenAIEndpoint                 – https://<resource>.openai.azure.com/
    //   Parameters:AzureOpenAIApiKey                   – API key (stored as a secret)
    //   Parameters:AzureOpenAIDeploymentName           – chat deployment, e.g. gpt-4.1-mini
    //   Parameters:AzureOpenAIEmbeddingsDeploymentName – embeddings deployment, e.g. text-embedding-ada-002
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoaiEndpoint = builder.AddParameter("AzureOpenAIEndpoint");
    var aoaiApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
    var aoaiChatDeployment = builder.AddParameter("AzureOpenAIDeploymentName");
    var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

    products
        .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
        .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
        .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
        .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment)
        .WithEnvironment("AI_UseGitHubModels", "false")
        .WithReference(appInsights);

    store.WithReference(appInsights)
        .WithExternalHttpEndpoints();
}
else
{
    // Local development: GitHub Models (https://github.com/marketplace/models).
    // Set these in eShopAppHost user-secrets:
    //   Parameters:GitHubModelsToken           – GitHub PAT with Models scope (required)
    //   Parameters:GitHubModelsEndpoint        – defaults to https://models.inference.ai.azure.com
    //   Parameters:GitHubModelsChatModel       – defaults to gpt-4.1-mini
    //   Parameters:GitHubModelsEmbeddingsModel – defaults to text-embedding-3-small
    var githubToken = builder.AddParameter("GitHubModelsToken", secret: true);
    var githubEndpoint = builder.AddParameter("GitHubModelsEndpoint", "https://models.inference.ai.azure.com");
    var githubChatModel = builder.AddParameter("GitHubModelsChatModel", "gpt-4.1-mini");
    var githubEmbedModel = builder.AddParameter("GitHubModelsEmbeddingsModel", "text-embedding-3-small");

    products
        .WithEnvironment("AI_UseGitHubModels", "true")
        .WithEnvironment("GitHubModelsToken", githubToken)
        .WithEnvironment("GitHubModelsEndpoint", githubEndpoint)
        .WithEnvironment("GitHubModelsChatModel", githubChatModel)
        .WithEnvironment("GitHubModelsEmbeddingsModel", githubEmbedModel);
}

builder.Build().Run();
