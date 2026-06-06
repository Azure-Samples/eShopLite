# A2A eShopLite Demo – Agent-to-Agent Orchestration with the Official Microsoft A2A .NET SDK

## Overview

This project demonstrates a practical Agent2Agent (A2A) scenario using the eShopLite sample as a foundation. The implementation showcases how multiple autonomous agents (microservices) can collaborate to fulfill complex business requests through the official Microsoft [A2A .NET SDK](https://github.com/a2aproject/a2a-dotnet).

---

## Key Features

- **A2A .NET SDK Integration:** Leverages the [A2A NuGet package](https://www.nuget.org/packages/A2A/) for agent-to-agent communication.
- **Three Autonomous Agents:** Inventory, Promotions, and Researcher agents, each with dedicated APIs and A2A message handlers.
- **Orchestration Service:** The Products API orchestrates agent calls in parallel, aggregates results, and returns enriched product data.
- **Blazor Frontend:** Store application with a search page supporting A2A search and enhanced results display.
- **Aspire Integration:** Uses .NET Aspire for service orchestration and deployment.
- **Comprehensive Unit Tests:** Validates A2A orchestration and agent interactions.

---

## Architecture

### High-Level Components

1. **Store (Frontend):** Blazor web application providing the user interface.
2. **Products API:** Main backend API that orchestrates agent communication using the A2A .NET SDK.
3. **Three Autonomous Agents:**
   - **Inventory Agent:** Provides real-time stock levels.
   - **Promotions Agent:** Supplies active promotions and discounts.
   - **Researcher Agent:** Delivers product insights and reviews.

### Architectural Diagram

```text
┌────────────┐      ┌──────────────┐      ┌────────────────────┐
│  Store UI  │─────▶│ Products API │─────▶│  A2A Orchestration │
└────────────┘      └──────────────┘      └─────────┬──────────┘
                                                    │
        ┌───────────────────────────────┬───────────┴───────────────┐
        │                               │                           │
┌───────────────┐              ┌────────────────┐         ┌──────────────────┐
│ Inventory     │              │ Promotions     │         │ Researcher       │
│ Agent         │              │ Agent          │         │ Agent            │
└───────────────┘              └────────────────┘         └──────────────────┘
```

---

## A2A Orchestration Flow

### Sequence Diagram

```text
User
 │
 │ 1. Search (A2A)
 ▼
Store (UI)
 │
 │ 2. /api/a2asearch/{search}
 ▼
Products API
 │
 │ 3. Find products
 │
 │ 4. For each product:
 │     ├─▶ InventoryAgent.HandleInventoryCheckAsync
 │     ├─▶ PromotionsAgent.HandlePromotionsAsync
 │     └─▶ ResearcherAgent.HandleInsightsAsync
 │
 │ 5. Aggregate responses
 ▼
Store (UI)
 │
 │ 6. Display enriched results
```

---

## Implementation Details

- **A2A .NET SDK:** [GitHub](https://github.com/a2aproject/a2a-dotnet) | [NuGet](https://www.nuget.org/packages/A2A/) | [Overview Blog](https://devblogs.microsoft.com/foundry/building-ai-agents-a2a-dotnet-sdk/)
- **Agent Skills:** Each agent defines its capabilities using `AgentSkill`.
- **Message Handling:** Agents implement message handlers (e.g., `HandleInventoryCheckAsync`) to process A2A messages.
- **Parallel Calls:** The Products API orchestrates parallel calls to all agents and aggregates their responses.
- **Service Registration:** Agents and orchestration services are registered in the DI container.

---

## Getting Started

### Prerequisites

- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [Visual Studio Code](https://code.visualstudio.com/Download) or [Visual Studio](https://visualstudio.microsoft.com/downloads/)
  - If using Visual Studio Code, install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- .NET Aspire workload ([setup guide](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire))
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/)
- [.NET Aspire CLI](https://aspire.dev/) (`dotnet tool install -g aspire.cli`) — used to set local secrets

### Configure Azure OpenAI secrets

This scenario reads its Azure OpenAI configuration from Aspire AppHost parameters. Set them as local
secrets with the [`aspire secret`](https://aspire.dev/reference/cli/commands/aspire-secret/) command,
targeting this scenario's AppHost project:

```shell
aspire secret set Parameters:AzureOpenAIEndpoint "https://<your-resource>.openai.azure.com/" --apphost ./src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIApiKey "<your-api-key>" --apphost ./src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIDeploymentName "gpt-4.1-mini" --apphost ./src/eShopAppHost/eShopAppHost.csproj
aspire secret set Parameters:AzureOpenAIEmbeddingsDeploymentName "text-embedding-3-small" --apphost ./src/eShopAppHost/eShopAppHost.csproj
```

> Tip: run `scripts\Set-AzureOpenAISecrets.ps1` from the repo root to set these four common Azure OpenAI
> values for every scenario at once (use `-DryRun` to preview).

### Running Locally

1. Navigate to the AppHost project:

   ```shell
   cd ./src/eShopAppHost/
   ```

2. (Codespaces only) Trust HTTPS dev certs:

   ```shell
   dotnet dev-certs https --trust
   ```

3. Run the solution:

   ```shell
   dotnet run
   ```

4. Open the Store app in your browser, go to the Search page, select "A2A Search (Agent-to-Agent)", and search for products.

### Deploying to Azure

1. Login to Azure:

   ```shell
   azd auth login
   ```

2. Provision and deploy resources:

   ```shell
   azd up
   ```

3. Follow the prompts for environment name, subscription, and region.
4. When deployment completes, visit the Store URI to use the app.

---

## Data Model Example

```json
{
  "response": "Found X products enriched with inventory, promotions, and insights data.",
  "products": [
    {
      "productId": "1",
      "name": "Product Name",
      "description": "Product Description",
      "price": 99.99,
      "imageUrl": "image.jpg",
      "stock": 42,
      "promotions": [
        { "title": "Special Offer", "discount": 15 }
      ],
      "insights": [
        { "review": "Great product!", "rating": 4.5 }
      ]
    }
  ]
}
```

---

## References & Resources

- [A2A .NET SDK GitHub](https://github.com/a2aproject/a2a-dotnet)
- [A2A .NET SDK NuGet](https://www.nuget.org/packages/A2A/)
- [A2A .NET SDK Overview Blog](https://devblogs.microsoft.com/foundry/building-ai-agents-a2a-dotnet-sdk/)
- [eShopLite Main Repository](../..)
- [Deploy a .NET Aspire project to Azure Container Apps using the Azure Developer CLI (in-depth guide)](https://learn.microsoft.com/dotnet/aspire/deployment/azure/aca-deployment-azd-in-depth)

---

*This scenario is part of the eShopLite sample, demonstrating advanced agent orchestration patterns for .NET developers using the official A2A .NET SDK.*
