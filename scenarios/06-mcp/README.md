<!-- filepath: d:\azure-samples\eShopLite\scenarios\06-mcp\README.md -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![Twitter: elbruno](https://img.shields.io/twitter/follow/elbruno.svg?style=social)](https://twitter.com/elbruno)
![GitHub: elbruno](https://img.shields.io/github/followers/elbruno?style=social)

## Description

**eShopLite - Model Context Protocol (MCP)** is a reference .NET application implementing an eCommerce site with advanced AI integration using the Model Context Protocol (MCP). This scenario demonstrates how to build and use MCP Servers and Clients to enhance search capabilities with access to external tools and services.

- [Features](#features)
- [Architecture diagram](#architecture-diagram)
- [Getting started](#getting-started)
- [Deploying to Azure](#deploying)
- Run solution
  - [Run locally](#run-locally)
  - [Run the solution](#run-the-solution)
  - [.NET Aspire Azure Resources creation](#net-aspire-azure-resources-creation)
  - [Local development using an existing model](#local-development-using-an-existing-model)
  - [Telemetry with .NET Aspire and Azure Application Insights](#telemetry-with-net-aspire-and-azure-application-insights)
- [Resources](#resources)
- [Guidance](#guidance)
  - [Costs](#costs)
  - [Security Guidelines](#security-guidelines)

## Features

**GitHub CodeSpaces:** This project is designed to be opened in GitHub Codespaces as an easy way for anyone to deploy the solution entirely in the browser.

This scenario showcases the following key capabilities:

- **MCP Server Implementation**: Demonstrates how to create a Model Context Protocol server using the [MCP C# SDK](https://modelcontextprotocol.github.io/csharp-sdk/), enabling AI models to interact with external tools
- **MCP Client Integration**: Shows how to build client applications that communicate with MCP servers
- **Tool Registration**: How to register and expose various tools to large language models
- **Configurable Tool Access**: Users can enable or disable specific MCP servers to access:
  - Weather information
  - Park and camping conditions
  - Online search capabilities
- **Server-Sent Events (SSE)**: Implementation of SSE for real-time communication between clients and servers

The MCP implementation enables large language models to extend their capabilities by accessing external tools and services, making the AI interaction more powerful and useful in the eCommerce context.

## Architecture diagram

```mermaid
---
title: eShopLite - Model Context Protocol (MCP)
---
architecture-beta

    group diagram(cloud)[Azure Deployment]

    group acapps(cloud)[Azure Container Apps Environment]

    service Store(cloud)[Store Blazor WebApp] in acapps
    service EShopMcpServer(server)[eShopMcpSseServer MCP Server] in acapps
    service Products(server)[Products Service] in acapps
    service OnlineResearcher(server)[OnlineResearcher] in acapps
    service ParkInfoAgent(server)[ParkInformation Agent] in acapps
    service WeatherAgent(server)[Weather Agent] in acapps
    service Sql(database)[SQL Server Database] in acapps    

    service ContainerRegistry(cloud)[Container Registry] in diagram
    service ManagedIdentity(server)[Managed Identity] in diagram
    service StorageAccount(disk)[Storage Account] in diagram
    service AzureOpenAI(cloud)[Azure OpenAI] in diagram
    service AppInsights(internet)[Application Insights] in diagram
    service AzureOpenAIFoundry(cloud)[Azure AI Foundry Project] in diagram

    Store:R <--> L:EShopMcpServer
    EShopMcpServer:R -- R:Products
    EShopMcpServer:R -- R:OnlineResearcher
    EShopMcpServer:R -- R:ParkInfoAgent
    EShopMcpServer:R -- R:WeatherAgent
    Products:R -- R:Sql
```

## Getting Started

The solution is in the `./src` folder, the main solution is **[eShopLite-Mcp.sln](./src/eShopLite-Mcp.sln)**.

## Deploying

Once you've opened the project in [Codespaces](#github-codespaces), or [locally](#run-locally), you can deploy it to Azure.

From a Terminal window, open the folder with the clone of this repo and run the following commands.

1. Login to Azure:

    ```shell
    azd auth login
    ```

2. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "eShopLite-MCP"), select a subscription from your Azure account, and select a [location where the necessary models, like gpt-4.1-mini and ADA-002 are available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all), a sample region can be "eastus2".

3. When `azd` has finished deploying, you'll see the list of resources created in Azure and a set of URIs in the command output.

4. Visit the **store** URI, and you should see the **eShop Lite app** with MCP capabilities! 🎉

### GitHub CodeSpaces

- Create a new Codespace using the `Code` button at the top of the repository.

- The Codespace creation process can take a couple of minutes.

- Once the Codespace is loaded, it should have all the necessary requirements to deploy the solution.

### Run Locally

To run the project locally, you'll need to make sure the following tools are installed:

- [.NET 9](https://dotnet.microsoft.com/downloads/)
- [Git](https://git-scm.com/downloads)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [Visual Studio Code](https://code.visualstudio.com/Download) or [Visual Studio](https://visualstudio.microsoft.com/downloads/)
  - If using Visual Studio Code, install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
  Installed with the [Visual Studio installer](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire) or the [.NET CLI workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire).
  Installed with the [Visual Studio installer](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire) or the [.NET CLI workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire).
- An OCI compliant container runtime, such as:
  - [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/).

### Run the solution

Follow these steps to run the project, locally or in CodeSpaces:

1. Navigate to the Aspire Host folder project using the command:

   ```bash
   cd ./src/eShopAppHost/
   ```

2. If you are running the project in Codespaces, you need to run this command:

   ```bash
   dotnet dev-certs https --trust
   ```

3. By default the AppHost project creates the necessary resources on Azure. Check the **[.NET Aspire Azure Resources creation](#net-aspire-azure-resources-creation)** section to learn how to configure the project to create Azure resources.

4. Run the project:

   ```bash
   dotnet run
   ```

## Understanding the Model Context Protocol (MCP)

The Model Context Protocol (MCP) is an open standard for interacting with Large Language Models (LLMs). It enables:

1. **Tool Registration**: Register tools that LLMs can use to interact with external services
2. **Function Calling**: Allow LLMs to call specific functions based on user queries
3. **Streaming Responses**: Support for real-time streaming of AI responses

In this eShopLite scenario, MCP is used to extend the capabilities of the search functionality by allowing the AI to:

- Access weather information relevant to outdoor products
- Check park and camping conditions when suggesting camping equipment
- Perform online searches to supplement product recommendations

### MCP C# SDK Implementation

The eShopLite MCP implementation uses the [MCP C# SDK](https://modelcontextprotocol.github.io/csharp-sdk/) to create both MCP servers and clients:

```csharp
// Example of MCP server registration with tool
[McpServerTool(Name = "OnlineSearch"), Description("Performs a search online")]
public static async Task<ProductsSearchToolResponse> OnlineSearch(
    ILogger<ProductService> logger,
    OnlineResearcherService researcherService,
    ChatClient chatClient,
    ProductService productService,
    [Description("The search query")] string query)
{
    // Implementation details
}
```

Users can enable or disable specific MCP servers through the interface, giving them control over which external data sources the AI can access:

```csharp
// Example of configuring available tools
private IList<ModelContextProtocol.Client.McpClientTool> tools;

protected override void OnInitialized()
{
    tools = McpServerService.GetTools();
}

private async Task DoSearch(MouseEventArgs e)
{
    if (mcpSearch)
    {
        // mcp search with selected tools
        searchResponse = await McpServerService.Search(searchTerm);
    }
    else
    {
        // keyword search
        searchResponse = await ProductService.Search(searchTerm);
    }
}
```

## .NET Aspire Azure Resources creation
The eShopLite MCP solution leverages .NET Aspire to create and manage the necessary Azure resources for deployment. For information on how .NET Aspire creates Azure resources, check the [.NET Aspire Azure hosting integrations](https://learn.microsoft.com/dotnet/aspire/azure/local-provisioning#net-aspire-azure-hosting-integrations) documentation.
The eShopLite MCP solution leverages .NET Aspire to create and manage the necessary Azure resources for deployment. For information on how .NET Aspire creates Azure resources, check the [.NET Aspire Azure hosting integrations](https://learn.microsoft.com/dotnet/aspire/azure/local-provisioning#net-aspire-azure-hosting-integrations) documentation.

### Local development using an existing model

To use existing AI models (like gpt-4.1-mini) for local development, you can configure the connection string using user secrets:

```bash
cd src/Products
dotnet user-secrets set "ConnectionStrings:openaidev" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<key>;"
```

### Telemetry with .NET Aspire and Azure Application Insights

The eShopLite MCP solution leverages the Aspire Dashboard and Azure Application Insights to provide comprehensive telemetry and monitoring capabilities for both the MCP servers and clients.

## Resources

- [Model Context Protocol (MCP) Specification](https://github.com/modelcontextprotocol/mcp)
- [MCP C# SDK Documentation](https://modelcontextprotocol.github.io/csharp-sdk/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Generative AI for Beginners .NET](https://aka.ms/genainnet)

## Guidance

### Costs

For **Azure OpenAI Services**, pricing varies per region and usage, so it isn't possible to predict exact costs for your usage. The MCP implementation may result in higher token usage due to function calling and interactions with external tools.

Key Azure resources used in this implementation:

- Azure OpenAI Service: S0 tier, gpt-4.1-mini model. Pricing is based on token count. [Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
- Azure Container App: Consumption tier with 0.5 CPU, 1GiB memory/storage. [Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- Azure Application Insights: Pay-as-you-go tier. [Pricing](https://azure.microsoft.com/pricing/details/monitor/)

⚠️ To avoid unnecessary costs, remember to take down your app if it's no longer in use, either by deleting the resource group in the Portal or running `azd down`.

### Security Guidelines

This sample uses Azure OpenAI Services with ApiKey and [Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

When implementing MCP servers that access external services, consider:

1. Implementing proper authentication for access to external APIs
2. Rate limiting to prevent excessive API calls
3. Input validation to prevent injection attacks
4. Access control for sensitive tool functions