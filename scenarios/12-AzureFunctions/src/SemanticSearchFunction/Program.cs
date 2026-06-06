using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SemanticSearchFunction.Functions;
using SemanticSearchFunction.Repositories;
using System.ClientModel;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // add db with support for vector search
        var productsDbConnectionString = context.Configuration.GetConnectionString("productsDb");
        services.AddDbContext<Context>(options =>
            options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));

        // Read 4 explicit Azure OpenAI parameters wired from AppHost via WithEnvironment.
        var endpoint = context.Configuration["AzureOpenAIEndpoint"] ?? "";
        var apiKey = context.Configuration["AzureOpenAIApiKey"] ?? "";
        var embeddingsDeploymentName = context.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-3-small";

        if (!string.IsNullOrEmpty(endpoint))
        {
            AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
                ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
                : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

            services.AddSingleton(aoaiClient);
            services.AddEmbeddingGenerator(
                aoaiClient.GetEmbeddingClient(embeddingsDeploymentName).AsIEmbeddingGenerator());
        }

        // Register ISemanticSearchRepository
        services.AddScoped<ISemanticSearchRepository>(sp =>
        {
            var dbContext = sp.GetRequiredService<Context>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var logger = sp.GetRequiredService<ILogger<SearchFunction>>();
            return new SqlSemanticSearchRepository(embeddingGenerator, dbContext, logger);
        });
    })
    .Build();

host.Run();