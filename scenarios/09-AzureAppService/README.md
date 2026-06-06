## Want to know more?

See [docs/README.md](./docs/README.md) for detailed architecture, feature documentation, and screenshots.
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)

## Scenario: Deploying .NET Aspire Apps to Azure App Service

This scenario demonstrates how to deploy a .NET Aspire-based eShopLite application to **Azure App Service**, leveraging the latest platform features announced at Microsoft Build 2025. It is a practical demo for modernizing and running .NET Aspire multi-service applications on Azure's fully managed web hosting platform.

### What's New: .NET Aspire on Azure App Service

- **.NET Aspire support is now available in public preview for App Service on Linux!**
- Developers can deploy multi-app/multi-service .NET Aspire applications directly to Azure App Service using the new Aspire App Service deployment provider.
- The App Service provider translates your .NET Aspire application's topology into Azure App Service resources, supporting secure deployments and observability (with Aspire dashboard integration coming soon).
- Take advantage of the new [Premium v4 plan](https://aka.ms/Build25/blog/Premiumv4) for enhanced performance, cost savings, and Availability Zone support.

For more details, see the [official announcement](https://techcommunity.microsoft.com/blog/appsonazureblog/whats-new-in-azure-app-service-at-msbuild-2025/4412465) and [Getting Started with .NET Aspire on Azure App Service](https://aka.ms/Build25/blog/AspireAppService).

---

**eShopLite - Semantic Search deploy to Azure App Service** is a reference .NET Aspire application implementing an eCommerce site with advanced search features, now ready for deployment to Azure App Service.

- [Features](#features)
- [Architecture diagram](#architecture-diagram)
- [Getting started](#getting-started)
- Run solution
- [Resources](#resources)
- [Video Recordings](#video-recordings)
- [Guidance](#guidance)
  - [Costs](#costs)
  - [Security Guidelines](#security-guidelines)
- [Resources](#resources)

## Features

**GitHub CodeSpaces:** This project is designed to be opened in GitHub Codespaces as an easy way for anyone to deploy the solution entirely in the browser.

The Azure Resources provisioned by this scenario includes the Products and the Store services, which are deployed to Azure App Service:

![ Products and the Store services, which are deployed to Azure App Service](./images/10AzureResources.png)

This is the eShopLite Application running, performing a **Keyword Search**:

![eShopLite Application running doing search using keyword search](./images/05eShopLite-SearchKeyWord.gif)

This is the eShopLite Application running, performing a **Semantic Search**:

![eShopLite Application running doing search using keyword search](./images/06eShopLite-SearchSemantic.gif)

## Architecture diagram

  ```mermaid
  flowchart TD
    subgraph "Azure App Service"
      store[Store Service]
      products[Products Service SQLite DB internal]
    end
    ContainerRegistry[Container Registry]
    StorageAccount[Storage Account]
    ManagedIdentity[Managed Identity]
    AzureOpenAI[Azure OpenAI Chat + Embeddings]
    InMemoryVectorDB[InMemory Vector DB]

    store --> products
    store --> ManagedIdentity

    ManagedIdentity --> ContainerRegistry
    ManagedIdentity --> StorageAccount
    ManagedIdentity --> AzureOpenAI
    AzureOpenAI --> InMemoryVectorDB

    products --> ManagedIdentity
    products -->|semantic search| InMemoryVectorDB
    products -->|generate embeddings + chat| AzureOpenAI
  ```

## Getting Started

## Main Concepts in this Scenario

This scenario demonstrates how to use a .NET Aspire application to Azure to AppServices. The main concepts and implementation details are:

The solution is in the `./src` folder, the main solution is **[eShopLite-Aspire-AppService.slnx](./src/eShopLite-Aspire-AppService.slnx)**.

## Deploying

> **Note:** The deployment process for this scenario is the same as in [scenario 01 - Semantic Search](../01-SemanticSearch/README.md). Please refer to the [Deploying](../01-SemanticSearch/README.md#deploying) section in that README for detailed steps and requirements.

## Local development using existing models

To run locally with an existing Azure OpenAI service, set the parameters as user secrets in the `eShopAppHost` project:

```bash
cd src/eShopAppHost

dotnet user-secrets set "Parameters:AzureOpenAIEndpoint" "https://<your-resource>.openai.azure.com/"
dotnet user-secrets set "Parameters:AzureOpenAIApiKey" "<your-api-key>"
dotnet user-secrets set "Parameters:AzureOpenAIDeploymentName" "gpt-4.1-mini"
dotnet user-secrets set "Parameters:AzureOpenAIEmbeddingsDeploymentName" "text-embedding-ada-002"
```

## Guidance

### Costs

For **Azure OpenAI Services**, pricing varies per region and usage, so it isn't possible to predict exact costs for your usage.
The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers.
However, Azure Container Registry has a fixed cost per registry per day.

You can try the [Azure pricing calculator](https://azure.com/e/2176802ea14941e4959eae8ad335aeb5) for the resources:

- Azure OpenAI Service: S0 tier, gpt-4.1-mini and text-embedding-3-small models. Pricing is based on token count. [Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
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
