# eShopLite

[![GitHub license](https://img.shields.io/github/license/azure-samples/eshoplite.svg)](https://github.com/Azure-Samples/eShopLite/blob/main/LICENSE.md)
[![GitHub contributors](https://img.shields.io/github/contributors/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/graphs/contributors/)
[![GitHub issues](https://img.shields.io/github/issues/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/issues/)
[![GitHub pull-requests](https://img.shields.io/github/issues-pr/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/pulls/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

[![GitHub watchers](https://img.shields.io/github/watchers/azure-samples/eshoplite.svg?style=social&label=Watch)](https://github.com/Azure-Samples/eShopLite)
[![GitHub forks](https://img.shields.io/github/forks/azure-samples/eshoplite.svg?style=social&label=Fork)](https://github.com/azure-samples/eshoplite/network/)
[![GitHub stars](https://img.shields.io/github/stars/azure-samples/eshoplite.svg?style=social&label=Star)](https://github.com/Azure-Samples/eShopLite)

[![Azure AI Community Discord](https://img.shields.io/badge/Discord-Azure%20AI-5865F2?logo=discord)](https://aka.ms/ai-discord/dotnet)

[![Azure AI Foundry GitHub Discussions](https://img.shields.io/badge/Discussions-Azure%20AI%20Foundry-blueviolet?logo=github&style=for-the-badge)](https://aka.ms/ai-discussions/dotnet)

**eShopLite** is a set of reference .NET applications implementing an eCommerce site with features like Semantic Search, Model Context Protocol (MCP), Reasoning models, vector databases, and more.

> - ☁️ **Scenarios use .NET and Aspire, with a stable .NET 10 baseline and an isolated .NET 11 RC2 scenario for experimental Blazor AI components. See the dependency baseline below for compatibility exceptions.**
> - 🌟 Don't forget to **[star (🌟) this repo](https://docs.github.com/en/get-started/exploring-projects-on-github/saving-repositories-with-stars)** to find it easier later.
> - ➡️ Get your own copy by **[Forking this repo](https://github.com/azure-samples/eshoplite/fork)** and find it next in your own repositories.
> - ❓ **Have a question?** Besides creating [issues](https://github.com/azure-samples/eshoplite/issues) or [pull requests](https://github.com/azure-samples/eshoplite/pulls), the best option for questions is to join the [Azure AI Discord channel](https://discord.com/invite/ByRwuEEgH4), where a team of AI experts can help you.

## Features

This project framework provides the following features:

- Modern .NET application architecture with Aspire
- Various search capabilities (keyword search, semantic search)
- Integration with multiple AI models (GPT-4o, DeepSeek-R1, etc.)
- Vector database implementations (In Memory, Azure AI Search, Chroma DB and others)
- Real-time audio capabilities
- Model Context Protocol (MCP) server and client implementation

## eShopLite Scenarios

The project includes several scenarios demonstrating different capabilities:

| Scenario | Description | Key Technologies |
|----------|-------------|-----------------|
| [01 - Semantic Search](./scenarios/01-SemanticSearch/) | A reference .NET application implementing an eCommerce site with Search features using Keyword Search and Semantic Search. | Aspire, OpenAI GPT-4.1-mini, In-memory Vector DB |
| [02 - Azure AI Search](./scenarios/02-AzureAISearch/) | Implements an eCommerce site with Keyword Search using SQL queries and Semantic Search with Vector Database and Azure AI Search. | Azure AI Search, OpenAI Embeddings, SQL Server |
| [03 - Realtime Audio](./scenarios/03-RealtimeAudio/) | Extends the eCommerce site with advanced search features and real-time audio capabilities powered by the GPT-4o Realtime Audio API. | GPT-4o Realtime Audio API, Audio in Blazor, Aspire |
| [04 - Chroma DB](./scenarios/04-chromadb/) | Implements semantic search functionality using Chroma DB, an open-source database designed for AI applications. | Chroma DB, OpenAI Embeddings, .NET SDK |
| [05 - DeepSeek-R1](./scenarios/05-deepseek/) | Demonstrates integration of the DeepSeek-R1 model for enhanced semantic understanding and search capabilities. | DeepSeek-R1, Aspire, Vector Embeddings |
| [06 - Model Context Protocol (MCP)](./scenarios/06-mcp/) | Implements the Model Context Protocol (MCP) for advanced AI interactions with MCP Servers and MCP Clients. | Model Context Protocol, Function Calling, SSE Events |
| [07 - Agents Concurrent](./scenarios/07-AgentsConcurrent/) | Demonstrates concurrent agent orchestration and advanced AI agent collaboration patterns. | Aspire, Multi-Agent Systems, Orchestration |
| [08 - SQL Server 2025](./scenarios/08-Sql2025/) | Demonstrates the use of vector search and vector indexes in the SQL Database Engine | SQL Server 2025, Vector Search, Vector Indexes |
| [09 - Azure App Service](./scenarios/09-AzureAppService/) | Shows how to deploy an Aspire multi-service eCommerce app to Azure App Service, using SQLite for data and integrating AI search. | Azure App Service, Aspire, OpenAI, SQLite |
| [10 - A2A Network](./scenarios/10-A2ANet/) | Demonstrates advanced agent-to-agent (A2A) communication and orchestration patterns in Aspire, including multi-agent collaboration and reasoning. | Aspire, Multi-Agent Systems, A2A Protocol |
| [11 - GitHub Models](./scenarios/11-GitHubModels/) | Local-first AI development using GitHub Models during local runs, with automatic switch to Azure OpenAI when deployed. | Aspire, GitHub Models, Azure OpenAI |
| [12 - Azure Functions](./scenarios/12-AzureFunctions/) | Optional Azure Functions façade for semantic search and an alternate deployment boundary for vector search. | Azure Functions, Aspire, Azure OpenAI |
| [13 - Observability Assistant with Foundry Local](./scenarios/13-ObservabilityAssistantFoundryLocal/) | Summarizes logs, traces, and incidents with a local-first observability assistant. | Aspire, OpenTelemetry, Foundry Local, Microsoft.Extensions.AI |
| [14 - Product Discovery Copilot](./scenarios/14-ProductDiscoveryCopilot/) | Turns search into natural-language product discovery with grounded explanations. | Semantic search, vector search, Microsoft.Extensions.AI |
| [15 - Store Intelligence Report](./scenarios/15-StoreIntelligenceReport/) | Generates daily business and operational store intelligence reports. | App data, telemetry, AI summarization, reports |
| [16 - MCP Store Operations Tools](./scenarios/16-MCPStoreOperationsTools/) | Exposes safe store capabilities as MCP tools for agent use. | MCP, Aspire, app APIs, tool calling |
| [17 - A2A Store Operations Network](./scenarios/17-A2AStoreOperationsNetwork/) | Shows specialized agents collaborating around the store app through A2A. | A2A, Microsoft Agent Framework, hosted agents |
| [18 - Blazor Agentic UI](./scenarios/18-BlazorAgenticUI/) | Experimental streaming shopping assistant with catalog cards and human-approved cart actions. | .NET 11 RC2, Blazor AI components, AG-UI, Microsoft Agent Framework, Aspire |

New to Scenario 18? Start with its [three-step quick start](./scenarios/18-BlazorAgenticUI/README.md),
then explore the [screenshots](./scenarios/18-BlazorAgenticUI/docs/WALKTHROUGH.md) and
[blog-to-code walkthrough](./scenarios/18-BlazorAgenticUI/docs/CODE_WALKTHROUGH.md).

## Getting Started

### Prerequisites

- [.NET 10 SDK 10.0.402](https://dotnet.microsoft.com/download/dotnet/10.0), pinned in the root `global.json`
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd) (for Azure deployment)
- [Git](https://git-scm.com/downloads)
- [Aspire CLI](https://aspire.dev): install with `dotnet tool install -g Aspire.Cli --version 13.6.1` (used for setting local secrets via `aspire secret set`)

### Dependency baseline

The October 14, 2026 refresh updates the 17 existing scenarios other than `14-MAFDevUI`
to Aspire 13.6.1 and the latest compatible NuGet releases. The root SDK stays on stable
.NET 10; [Scenario 18](./scenarios/18-BlazorAgenticUI/) overrides it with .NET 11 RC2
for the experimental Blazor components. Install both SDKs to work on all scenarios.

The refresh includes Microsoft.Extensions.AI 10.11.0, OpenTelemetry 1.19,
Microsoft Agent Framework 1.24.0, MCP 2.2.0, and A2A 1.0.0-preview3 where used.
Existing prerelease dependencies remain prerelease where no stable replacement provides
the required functionality. MCP SSE transports and A2A text parts have been migrated to
the current APIs rather than merely changing package versions.

Scenarios 08 and 12 deliberately retain the latest EF Core 9 servicing release, 9.0.21:
their `EFCore.SqlServer.VectorSearch` 9.0.0 provider requires that EF major. The other
EF-backed scenarios use 10.0.13. Local `dotnet-ef` tool manifests follow the matching
EF major. Upgrading the SQL vector provider to native EF 10 requires a separate vector
entity/query migration, not mixing EF 9 relational assemblies with EF 10 tests.
Direct PKCS references select patched 10.0.13 instead of vulnerable transitive versions.

`14-MAFDevUI` has not been refreshed: its local instructions require an Azure
best-practices tool that is unavailable in this session. Its existing dependency versions
are retained until that prerequisite is supplied. Builds and existing tests passed for
the other 17 existing scenario solutions (80 existing tests), and Scenario 18 has 29
passing tests. Scenario 18 was also validated locally with SQL Server 2025, real Foundry
embeddings, and `gpt-6-luna` via Responses: streamed catalog cards, cart rejection/approval,
missing product specifications, and restart against existing data. These checks do not
establish cloud deployability or validate every other scenario against real models.

See [CHANGELOG.md](./CHANGELOG.md) for the new scenario, dependency migrations, setup
scripts, and known upgrade boundaries.

### Start a scenario locally

Each scenario has its own `aspire.config.json`, so Aspire selects that scenario's AppHost
instead of inheriting the repository-root selection:

```powershell
cd .\scenarios\18-BlazorAgenticUI
aspire start
```

Use `aspire run` for a foreground session, and `aspire stop` to stop the selected app.
Install the SDK requested by that scenario's nearest `global.json`: .NET 10.0.402 for
stable scenarios, or .NET 11 RC2 for Scenario 18. A .NET 11 installation does not satisfy
the stable .NET 10 pin. AppHost selection does not install SDKs or configure credentials.

To verify every scenario's local selection without starting services:

```powershell
pwsh .\scripts\Tests\Test-ScenarioAppHosts.ps1
```

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Azure-Samples/eShopLite.git
   ```

1. Navigate to the scenario directory of interest:

   ```bash
   cd eShopLite/scenarios/[scenario-folder]
   ```

1. Login to Azure:

    ```shell
    azd auth login
    ```

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "eShopLite"), select a subscription from your Azure account, and select a [location where the necessary models, like gpt-4.1-mini and ADA-002 are available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all), a sample region can be "eastus2".

### Shared Microsoft Foundry models and local secrets

Use PowerShell 7 and sign in with `az login`. The new setup script creates or reuses an
`AIServices` account, deploys the common chat and embedding models, then configures local
AppHost user-secrets through the Aspire CLI. Supply your own subscription, resource group,
globally unique account name, and region:

```powershell
pwsh .\scripts\Initialize-FoundryModels.ps1 `
  -SubscriptionId '<subscription-id>' `
  -ResourceGroupName 'rg-eshoplite' `
  -AccountName '<globally-unique-account-name>' `
  -Location 'eastus2' `
  -DryRun
```

Remove `-DryRun` to provision resources and write secrets. This can incur Azure charges.
`-DryRun` and `-WhatIf` make no Azure calls and do not establish model availability.

The default chat model and deployment name are **`gpt-6-luna`**; embeddings use
**`text-embedding-3-small`**. The script resolves model versions from the regional catalog
and checks SKU support and quota before provisioning. It stops if the requested model is
unavailable, with no automatic substitution. Override model/version, deployment name, SKU,
or capacity with the corresponding `-Chat*` and `-Embeddings*` parameters.
It reuses matching deployments and refuses to overwrite conflicting deployments or enable
key authentication on an account that disables it.

This provisions common model dependencies for local development, not every scenario's
infrastructure. SQL/containers remain managed by each AppHost; Azure AI Search, realtime
audio, DeepSeek, GitHub Models, and other specialized dependencies still need their
scenario-specific setup. It does not change model declarations used by `azd up`.

### Configure secrets for existing deployments

Secret setup requires the SDK selected by each AppHost's nearest `global.json`.
The script checks SDK availability before requesting a key. If using a nonstandard SDK
installation, pass `-DotNetRoot "<SDK installation directory>"`; the override applies
only to child commands. An Aspire exit code 7 can indicate a missing SDK, not an Azure
authentication problem. Scenario 18 specifically requires .NET 11 RC2.

The existing `scripts\Set-AzureOpenAISecrets.ps1` configures secrets only, without creating
Azure resources. It discovers every `eShopAppHost.csproj`, including layouts without `src`,
and automatically includes future scenarios. Scenario 18 (`18-BlazorAgenticUI`) has its own
AppHost and independent user-secrets store, and is included automatically. Directories
without an AppHost are reported as skipped. Run from the **repo root**:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1
```

The script interactively prompts for four values:

| Prompt | Parameter set |
|--------|---------------|
| Azure OpenAI endpoint | `Parameters:AzureOpenAIEndpoint` |
| Azure OpenAI API key *(masked)* | `Parameters:AzureOpenAIApiKey` |
| Chat deployment name (default `gpt-6-luna`) | `Parameters:AzureOpenAIDeploymentName` |
| Embeddings deployment name (default `text-embedding-3-small`) | `Parameters:AzureOpenAIEmbeddingsDeploymentName` |

Use `-DryRun` to preview the commands without executing them:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 -DryRun
```

For a preview without prompts or an API key:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 `
  -Endpoint 'https://<your-resource>.openai.azure.com/' -NonInteractive -DryRun
```

Non-interactive writes additionally require `-ApiKey` as a `SecureString`, supplied from a
masked prompt or a secure credential source, not a plaintext key in command history.
Local .NET user-secrets are not an encrypted production secret store.

Internally, the script calls the **Aspire CLI** (`aspire secret set`) for each AppHost it discovers. To set a single value manually, use the same command directly:

```bash
aspire secret set Parameters:AzureOpenAIEndpoint "https://<your-resource>.openai.azure.com/" \
  --apphost scenarios/01-SemanticSearch/src/eShopAppHost/eShopAppHost.csproj
```

> **Note:** Scenario-specific extra parameters (e.g., `Parameters:AzureOpenAIRealtimeDeploymentName` in 03-RealtimeAudio, `Parameters:DeepSeekEndpoint` in 05-deepseek, `Parameters:GitHubModelsToken` in 11-GitHubModels) must still be set manually. See each scenario's README for details.

### Quickstart

1. Navigate to a specific scenario folder (e.g., `scenarios/01-SemanticSearch/`)
2. Follow the README instructions in that scenario folder
3. Run the solution using `dotnet run` in the appropriate host project folder

## Demo

To run the demo, follow these steps:

1. Navigate to the specific scenario folder
2. Follow the "Run the solution" instructions in that scenario's README
3. Access the application via the URLs provided in the console output

## Sample Application

This is the eShopLite Aplication running, performing a **Keyword Search**:

![eShopLite Aplication running doing search using keyworkd search](./images/05eShopLite-SearchKeyWord.gif)

This is the eShopLite Aplication running, performing a **Semantic Search**:

![eShopLite Aplication running doing search using keyworkd search](./images/06eShopLite-SearchSemantic.gif)

This is the eShopLite Application running the **Realtime Audio** feature:

![eShopLite Application running the Realtime Audio feature](./images/07eShopLite-RealtimeAudio.gif)

This is the eShopLite Application using the **DeepSeek-R1 Reasoning Model**:

![eShopLite Application using the DeepSeek-R1 Reasoning Model](./images/07ReasoningSearch.png)

The Aspire Dashboard to check the running services:

![Aspire Dashboard to check the running services](./images/15AspireDashboard.png)

The Azure Resource Group with all the deployed services:

![Azure Resource Group with all the deployed services](./images/10AzureResources.png)

## Resources

- [Generative AI for Beginners .NET](https://aka.ms/genainet)

- [Aspire Documentation](https://aspire.dev/docs/)

- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Getting Help

If you get stuck or have questions about building AI apps, join:

[![Azure AI Foundry Discord](https://img.shields.io/badge/Discord-Azure_AI_Foundry_Community_Discord-blue?style=for-the-badge&logo=discord&color=5865f2&logoColor=fff)](https://aka.ms/foundry/discord)

If you have product feedback or errors while building, visit:

[![Azure AI Foundry Developer Forum](https://img.shields.io/badge/GitHub-Azure_AI_Foundry_Developer_Forum-blue?style=for-the-badge&logo=github&color=000000&logoColor=fff)](https://aka.ms/foundry/forum)
