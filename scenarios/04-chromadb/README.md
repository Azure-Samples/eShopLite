# eShopLite - Semantic Search with Chroma DB

eShopLite is a reference .NET application that implements an eCommerce site with advanced search features, including both keyword and semantic search capabilities. This version utilizes [Chroma DB](https://devblogs.microsoft.com/dotnet/announcing-chroma-db-csharp-sdk/), an open-source database designed for AI applications, to enhance semantic search functionality.

## Features

- **Keyword Search**: Traditional search based on exact keyword matches.
- **Semantic Search**: Leverages Chroma DB to understand the context and intent behind user queries, providing more relevant search results.
- **GitHub Codespaces Integration**: Easily deploy and run the solution entirely in the browser using GitHub Codespaces.

## Getting Started

The solution is in the `./src` folder, the main solution is **[eShopLite-ChromaDB.sln](./src/eShopLite-ChromaDB.sln)**.


## Deploying

Once you've opened the project in [Codespaces](#github-codespaces), or [locally](#run-locally), you can deploy it to Azure.

From a Terminal window, open the folder with the clone of this repo and run the following commands.

1. Login to Azure:

    ```shell
    azd auth login
    ```

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "eShopLite"), select a subscription from your Azure account, and select a [location where OpenAI the models gpt-4.1-mini and ADA-002 are available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) (like "eastus2").

1. When `azd` has finished deploying, you'll see the list of resources created in Azure and a set of URIs in the command output.

1. Visit the **store** URI, and you should see the **eShop Lite app**! 🎉

1. This is an example of the command output:

### GitHub CodeSpaces

- Create a new  Codespace using the `Code` button at the top of the repository.

- The Codespace creation process can take a couple of minutes.

- Once the Codespace is loaded, it should have all the necessary requirements to deploy the solution.

## Run Locally

### Prerequisites

- [.NET 9](https://dotnet.microsoft.com/downloads/)
- [Git](https://git-scm.com/downloads)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [Visual Studio Code](https://code.visualstudio.com/Download) or [Visual Studio](https://visualstudio.microsoft.com/downloads/)
  - If using Visual Studio Code, install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- .NET Aspire workload:
    Installed with the [Visual Studio installer](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire) or the [.NET CLI workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire).
- An OCI compliant container runtime, such as:
  - [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/).

### Chroma DB Overview

1. The AspireHost project will be in charge to **provision and run Chroma DB (locally for dev environments)**: 

   Instead of using the the Chroma Docker image to start a local instance of Chroma DB.

   ```bash
   docker run -p 8000:8000 chromadb/chroma
   ```

   Aspire Host will create a persistent container for Chroma DB, which will be used to store the embeddings and metadata for the products.

   ```csharp
    var chromaDB = builder.AddContainer("chroma", "chromadb/chroma")
        .WithHttpEndpoint(port: 8000, targetPort: 8000, name: "chromaendpoint")
        .WithLifetime(ContainerLifetime.Persistent);

    var endpoint = chromaDB.GetEndpoint("chromaendpoint");

    var products = builder.AddProject<Projects.Products>("products")
        .WithReference(endpoint)
        .WithReference(sqldb)
        .WaitFor(sqldb);
   ```

1. **Connect to Chroma DB in Your Application**: 

   The `ChromaDB.Client` NuGet package will allow the connections to the ChromaDB in your `Products` project.

   ```csharp
   using ChromaDB.Client;

   var chromaDbService = _config.GetSection("services:chroma:chromaendpoint:0");
   var chromaDbUri = chromaDbService.Value;

   var configOptions = new ChromaConfigurationOptions(uri: $"{chromaDbUri}/api/v1/");
   _httpChromaClient = new HttpClient();
   var client = new ChromaClient(configOptions, _httpChromaClient);
   ```

1. **Create a Collection**: Create a collection in Chroma DB to store your product data.

   ```csharp
   var collection = await client.GetOrCreateCollection("products");
   _collectionClient = new ChromaCollectionClient(collection, configOptions, _httpChromaClient);
   ```

1. **Add Data to the Collection**: Add your product data, including embeddings and metadata, to the collection.

   ```csharp
    var productIds = new List<string>();
    var productDescriptionEmbeddings = new List<ReadOnlyMemory<float>>();
    var productMetadata = new List<Dictionary<string, object>>();

    // iterate over the products and add them to the memory
    foreach (var product in products)
    {
        try
        {
            _logger.LogInformation("Adding product to memory: {Product}", product.Name);
            var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";
            var result = await _embeddingClient.GenerateEmbeddingAsync(productInfo);
            productIds.Add(product.Id.ToString());
            productDescriptionEmbeddings.Add(result.Value.ToFloats());
            _logger.LogInformation($"Product added to collections: {product.Name}");
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Error adding product to memory");
        }
    }

    // add the products to the memory
    await _collectionClient.Upsert(productIds, productDescriptionEmbeddings, productMetadata);

   ```

1. **Search products**: search the products using the Chroma DB client.

   ```csharp
    var resultGenEmbeddings = await _embeddingClient.GenerateEmbeddingAsync(search);
    var embeddingsSearchQuery = resultGenEmbeddings.Value.ToFloats();

    var searchOptions = new VectorSearchOptions
    {
        Top = 1,
        VectorPropertyName = "Vector"
    };

    // search the vector database for the most similar product        
    var queryResult = await _collectionClient.Query(
        queryEmbeddings: embeddingsSearchQuery,
        nResults: 2,
        include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances);

    var sbFoundProducts = new StringBuilder();
    int productPosition = 1;
    foreach (var result in queryResult)
    {
        if (result.Distance > 0.3)
        {
            // product found, magic happens here
        }
    }
   ```

### Running the Application

Follow these steps to run the project, locally or in CodeSpaces:

- Navigate to the Aspire Host folder project using the command:

  ```bash
  cd ./src/eShopAppHost/
  ```

- If you are running the project in Codespaces, you need to run this command:

  ```bash
  dotnet dev-certs https --trust
  ```

- By default the AppHost project creates the necessary resources on Azure. Check the **[.NET Aspire Azure Resources creation](#net-aspire-azure-resources-creation)** section to learn how to configure the project to create Azure resources.

- Run the project:

  ```bash
  dotnet run
  ````

Check the [Video Resources](#resources) for a step-by-step on how to run this project.

> **Note:** Working with .NET Aspire in GitHub Codespaces is not fully supported yet. As a developer you need to perform a lot of manual steps to access the .NET Aspire portal, like changing ports to public, copy the access token and more. The .NET Aspire version 9.1 will improve the whole developer experience. We will update these steps when the version 9.1 is released.

### Local development using an existing gpt-4.1-mini and ada-002 model

In order to use existing models: gpt-4.1-mini and text-embedding-ada-002, you need to define the specific connection string in the `Products` project.

- Configure the Application: Add user secrets in Products project to connect to Azure OpenAI services.

   ```bash
    dotnet user-secrets init
    dotnet user-secrets set "openai" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<ApiKey>;" 
   ```

## Resources

- [Chroma DB C# SDK Announcement](https://devblogs.microsoft.com/dotnet/announcing-chroma-db-csharp-sdk/)
- [eShopLite Semantic Search Sample](https://github.com/Azure-Samples/eShopLite-SemanticSearch)

## Guidance

### Costs

For **Azure OpenAI Services**, pricing varies per region and usage, so it isn't possible to predict exact costs for your usage.
The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers.
However, Azure Container Registry has a fixed cost per registry per day.

You can try the [Azure pricing calculator](https://azure.com/e/2176802ea14941e4959eae8ad335aeb5) for the resources:

- Azure OpenAI Service: S0 tier, gpt-4.1-mini and text-embedding-ada-002 models. Pricing is based on token count. [Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
- Azure Container App: Consumption tier with 0.5 CPU, 1GiB memory/storage. Pricing is based on resource allocation, and each month allows for a certain amount of free usage. [Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- Azure Container Registry: Basic tier. [Pricing](https://azure.microsoft.com/pricing/details/container-registry/)
- Log analytics: Pay-as-you-go tier. Costs based on data ingested. [Pricing](https://azure.microsoft.com/pricing/details/monitor/)
- Azure Application Insights pricing is based on a Pay-As-You-Go model. [Pricing](https://learn.microsoft.com/azure/azure-monitor/logs/cost-logs).

⚠️ To avoid unnecessary costs, remember to take down your app if it's no longer in use, either by deleting the resource group in the Portal or running `azd down`.

### Security Guidelines

Samples in this templates uses Azure OpenAI Services with ApiKey and [Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

The Main Sample uses Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

Additionally, we have added a [GitHub Action](https://github.com/microsoft/security-devops-action) that scans the infrastructure-as-code files and generates a report containing any detected issues. To ensure continued best practices in your own repository, we recommend that anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/code-security/secret-scanning/about-secret-scanning) setting is enabled.

You may want to consider additional security measures, such as:

- Protecting the Azure Container Apps instance with a [firewall](https://learn.microsoft.com/azure/container-apps/waf-app-gateway) and/or [Virtual Network](https://learn.microsoft.com/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

## Resources

- [Deploy a .NET Aspire project to Azure Container Apps using the Azure Developer CLI (in-depth guide)](https://learn.microsoft.com/dotnet/aspire/deployment/azure/aca-deployment-azd-in-depth)

- [Aspiring .NET Applications with Azure OpenAI](https://learn.microsoft.com/shows/azure-developers-dotnet-aspire-day-2024/aspiring-dotnet-applications-with-azure-openai)

### Video Recordings

[![Run eShopLite Semantic Search in Minutes with .NET Aspire & GitHub Codespaces 🚀](./images/90ytrunfromcodespaces.png)](https://youtu.be/T9HwjVIDPAE)