# eShopLite Copilot Instructions
## Project Overview
eShopLite is a modular Aspire-based reference eCommerce platform demonstrating advanced AI, search, and orchestration patterns. Each scenario in `scenarios/` is a self-contained solution, often integrating Azure, OpenAI, vector DBs, and/or agent protocols.
## Architecture & Patterns
- **Scenario-driven:** Each folder in `scenarios/` is a complete, runnable solution (see its README for details). Common patterns include:
  - **AppHost Orchestration:** All services (APIs, DBs, agents) are composed in `eShopAppHost` using Aspire's builder API.
  - **Service Boundaries:** Projects like `Products`, `Store`, and `VectorEntities` are reused across scenarios.
  - **AI Integration:** Semantic search, embeddings, and chat use Azure OpenAI, DeepSeek, or Chroma DB, configured via user-secrets or Aspire resource bindings.
  - **Agent/MCP Patterns:** Some scenarios (e.g., 06-mcp, 07-AgentsConcurrent) use Model Context Protocol or Semantic Kernel for tool/function calling and agent orchestration.
## Critical Workflows
- **Build/Run:**
  - Always run from the scenario's `src/eShopAppHost` using `dotnet run` (or the provided VS Code task).
  - For Azure deployment, use `azd up` from the scenario root after `azd auth login`.
  - Local dev often requires Docker/Podman for containerized DBs (e.g., Chroma, SQL 2025).
- **Configuration:**
  - Azure/OpenAI endpoints and keys are injected via user-secrets (see scenario README for exact keys).
  - Aspire auto-wires service URLs and credentials for local and cloud.
- **Testing:**
  - Unit/integration tests are in `Products.Tests`, `Store.Tests`, etc. Run with `dotnet test` from the scenario's `src`.
- **Telemetry:**
  - Aspire Dashboard and Azure Application Insights are integrated for all scenarios. Access dashboard URLs from console output after launch.
## Project-Specific Conventions
- **Aspire:** All orchestration is done in `eShopAppHost/Program.cs` using the Aspire builder API. Services are referenced by logical names (e.g., `products`, `store`, `sql`).
- **User Secrets:** For local dev, always set connection strings using `dotnet user-secrets` in the relevant project directory.
- **Azure Integration:**
  - Use `CredentialSource: AzureCli` for local Azure auth.
  - Resource provisioning is handled by Aspire or Bicep files in `infra/`.
- **AI/Vector DBs:**
  - Embedding and search clients are registered in code with `AddAzureOpenAIClient`, `AddAzureSearchClient`, or Chroma DB client.
  - For custom models (DeepSeek, GPT-4o, etc.), update connection strings and model names as shown in scenario READMEs.
- **Agent/MCP:**
  - MCP tools are registered via attributes and exposed to LLMs for function calling.
  - Agent orchestration (e.g., concurrent analysis) is implemented using Semantic Kernel or MCP SDK.
## Integration Points
- **External Services:**
  - Azure OpenAI, Azure AI Search, SQL Server 2025, Chroma DB, DeepSeek, and others are provisioned per scenario.
  - All external endpoints/keys are injected via Aspire or user-secrets.
- **Cross-component Communication:**
  - Services communicate via HTTP/gRPC, with endpoints auto-wired by Aspire.
  - MCP/agent scenarios use SSE or direct function calls for tool invocation.
## Examples
- To add a new AI model, update the relevant `user-secrets` and register the client in `Program.cs`:
  ```csharp
  var azureOpenAiClientName = "openaidev";
  builder.AddAzureOpenAIClient(azureOpenAiClientName);
  ```
- To run a scenario locally:
  ```pwsh
  cd scenarios/01-SemanticSearch/src/eShopAppHost
  dotnet run
  ```
- To deploy to Azure:
  ```pwsh
  azd auth login
  azd up
  ```
## References
- See each scenario's README for scenario-specific setup, configuration, and troubleshooting.
- Main docs: [README.md](../../README.md), [scenarios/*/README.md](../..)
---
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.