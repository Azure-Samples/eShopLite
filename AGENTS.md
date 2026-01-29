# AGENTS.md

## Project Overview

**eShopLite** is a modular .NET Aspire-based reference eCommerce platform demonstrating advanced AI, search, and orchestration patterns. This is a **scenario-driven monorepo** where each scenario in `scenarios/` is a self-contained solution showcasing different capabilities like Semantic Search, Model Context Protocol (MCP), Reasoning models, vector databases, real-time audio, and more.

**Key Technologies:**
- .NET 9 with .NET Aspire for orchestration
- Blazor for UI
- Azure OpenAI (GPT-4o, GPT-4.1-mini, embeddings)
- Vector databases (In-memory, Azure AI Search, Chroma DB, SQL Server 2025)
- Model Context Protocol (MCP) for agent interactions
- Azure services (Container Apps, AI Search, Application Insights, Functions)
- Docker/Podman for containerized services

**Architecture:** Each scenario is a complete, runnable .NET Aspire solution with an `eShopAppHost` orchestrating multiple services (APIs, databases, UI). Services are reused across scenarios, making this a true monorepo with shared components.

## Setup Commands

### Prerequisites Installation

Install the following prerequisites:

```bash
# Install .NET 9 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0

# Install Azure Developer CLI
# macOS/Linux:
curl -fsSL https://aka.ms/install-azd.sh | bash
# Windows (PowerShell):
# powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"

# Install Docker Desktop or Podman
# Docker: https://www.docker.com/products/docker-desktop/
# Podman: https://podman.io/

# Install .NET Aspire workload
dotnet workload install aspire

# Verify installations
dotnet --version  # Should be 9.0 or higher
azd version
docker --version  # or podman --version
```

### Clone and Navigate

```bash
# Clone the repository
git clone https://github.com/Azure-Samples/eShopLite.git
cd eShopLite

# Navigate to a specific scenario (e.g., Semantic Search)
cd scenarios/01-SemanticSearch
```

## Development Workflow

### Running a Scenario Locally

Each scenario is designed to run independently. The general pattern:

```bash
# Navigate to the scenario's AppHost
cd scenarios/<scenario-name>/src/eShopAppHost

# Run the solution
dotnet run

# The Aspire Dashboard will open in your browser
# Access URLs for services (Store, Products API, etc.) will be displayed in the console
```

**Example for Semantic Search:**

```bash
cd scenarios/01-SemanticSearch/src/eShopAppHost
dotnet run
```

**What happens:**
- .NET Aspire orchestrates all services (Store UI, Products API, databases)
- Aspire Dashboard opens at `http://localhost:15888` (or similar)
- Store UI is available at a dynamically assigned port (check console output)
- All service endpoints are auto-wired and displayed in the dashboard

### Hot Reload and Watch Mode

.NET 9 includes hot reload by default. Code changes in Blazor and API projects will auto-refresh without full restart.

### Configuration and Secrets

**Local development requires Azure OpenAI credentials:**

1. **Set user secrets in the AppHost project:**

```bash
cd scenarios/<scenario-name>/src/eShopAppHost

# Set OpenAI connection string (required for most scenarios)
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://<your-endpoint>.openai.azure.com/;Key=<your-key>"

# For Azure AI Search scenarios
dotnet user-secrets set "ConnectionStrings:azureaisearch" "Endpoint=https://<your-endpoint>.search.windows.net;Key=<your-key>"
```

2. **Or configure via Aspire Dashboard at first run:**
   - On first run, Aspire Dashboard will prompt for missing connection strings
   - Enter values in the dashboard UI
   - Values are stored as user secrets automatically

**For Azure deployment via `azd`:**
- Configuration is handled automatically
- Resources are provisioned with Managed Identity
- No manual secret management needed

## Testing Instructions

### Run All Tests

From the repository root or scenario root:

```bash
# Run all tests in the repository
dotnet test

# Run tests for a specific scenario
cd scenarios/01-SemanticSearch/src
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Projects

Test projects follow the pattern `<ProjectName>.Tests`:
- `Products.Tests` - Tests for Products API
- `Store.Tests` - Tests for Store UI

**Example - Run specific test project:**

```bash
cd scenarios/01-SemanticSearch/src/Products.Tests
dotnet test
```

**Example - Run specific test by name:**

```bash
dotnet test --filter "FullyQualifiedName~SemanticSearchTests"
```

### Testing Patterns

- Unit tests use xUnit framework
- Tests are located alongside the projects they test
- Follow existing test patterns when adding new tests
- Ensure tests pass before committing: `dotnet test`

## Code Style

### Formatting and Linting

**Before each commit:**

```bash
# Format all C# code (required)
dotnet format

# Verify formatting without making changes
dotnet format --verify-no-changes

# Format specific solution
cd scenarios/01-SemanticSearch/src
dotnet format eShopLite-Aspire.slnx
```

### Coding Conventions

- Follow C# and .NET 9 best practices
- Use modern C# features (top-level statements, nullable reference types, file-scoped namespaces)
- Follow async/await patterns consistently
- Use dependency injection for service registration
- Document public APIs and complex logic with XML comments
- Keep code accessible for beginners - add explanatory comments where helpful

### File Organization

- Each scenario has its own `src/` directory with all projects
- Shared entities: `DataEntities`, `VectorEntities`, `SearchEntities`, `CartEntities`
- Service projects: `Products` (API), `Store` (Blazor UI)
- Orchestration: `eShopAppHost` (Aspire host)
- Configuration: `eShopServiceDefaults` (shared Aspire settings)

### Naming Conventions

- Projects: PascalCase (e.g., `Products`, `VectorEntities`)
- Aspire resource names: lowercase (e.g., `products`, `store`, `sql`)
- Configuration sections: Follow .NET conventions (e.g., `ConnectionStrings`)

## Build and Deployment

### Build Commands

```bash
# Build entire repository
dotnet build

# Build specific scenario
cd scenarios/01-SemanticSearch/src
dotnet build

# Build specific project
cd scenarios/01-SemanticSearch/src/Products
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

### Local Development Build

```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore dependencies
dotnet restore
```

### Deploy to Azure

**Using Azure Developer CLI (azd):**

```bash
# Navigate to scenario root (where azure.yaml is located)
cd scenarios/01-SemanticSearch/src/eShopAppHost

# Login to Azure (first time only)
azd auth login

# Provision and deploy all resources
azd up
# You'll be prompted for:
# - Environment name (e.g., "eshoplite")
# - Azure subscription
# - Azure region (e.g., "eastus2")

# After successful deployment, URLs will be displayed in console output
```

**What `azd up` does:**
- Creates Azure resource group
- Provisions Azure OpenAI, Container Apps, AI Search, Application Insights, etc.
- Builds and containerizes services
- Deploys to Azure Container Apps
- Configures Managed Identity for secure access

**Other useful azd commands:**

```bash
# Deploy code changes without reprovisioning
azd deploy

# Tear down all Azure resources
azd down

# View environment configuration
azd env get-values

# Monitor deployed app
azd monitor
```

### Environment Configurations

- **Local:** Uses Docker containers, user secrets, local Aspire Dashboard
- **Azure:** Uses Azure Container Apps, Managed Identity, Azure Application Insights

## Monorepo Navigation

### Scenario Structure

```
scenarios/
├── 01-SemanticSearch/          # Keyword + semantic search with in-memory vector DB
├── 02-AzureAISearch/           # Azure AI Search integration
├── 03-RealtimeAudio/           # GPT-4o Realtime Audio API
├── 04-chromadb/                # Chroma DB vector database
├── 05-deepseek/                # DeepSeek-R1 reasoning model
├── 06-mcp/                     # Model Context Protocol servers/clients
├── 07-AgentsConcurrent/        # Multi-agent orchestration
├── 08-Sql2025/                 # SQL Server 2025 vector search
├── 09-AzureAppService/         # Azure App Service deployment
├── 10-A2ANet/                  # Agent-to-Agent (A2A) protocol
├── 11-GitHubModels/            # GitHub Models integration
├── 12-AzureFunctions/          # Azure Functions façade
└── 14-MAFDevUI/                # MAF Development UI
```

### Working with Multiple Scenarios

Each scenario is **independent and self-contained:**
- Has its own `src/` directory with all projects
- Has its own `azure.yaml` for deployment
- Has its own README.md with specific instructions
- Shares common project patterns (Products, Store, Entities)

**To switch scenarios:**

```bash
# Stop current scenario (Ctrl+C)
# Navigate to new scenario
cd ../02-AzureAISearch/src/eShopAppHost
dotnet run
```

**To work on multiple scenarios simultaneously:**
- Each scenario runs on different ports (managed by Aspire)
- Can run multiple scenarios in separate terminal windows
- Each has its own Aspire Dashboard instance

### Scenario-Specific Commands

Each scenario's README contains specific setup instructions. Always check:

```bash
cd scenarios/<scenario-name>
cat README.md
```

## Security Considerations

### Authentication and Secrets

- **Never commit secrets or API keys to source control**
- Use `dotnet user-secrets` for local development
- Use Azure Managed Identity for production (automatically configured by azd)
- Azure OpenAI keys are stored in user secrets or Key Vault

### Security Best Practices

- All scenarios use secure credential management
- GitHub Actions include security scanning (see `.github/workflows/codeql.yml`)
- Enable GitHub secret scanning in your fork
- Follow Azure OpenAI security guidelines
- Use HTTPS endpoints for all Azure services

### Recommended Additional Security

- Use Virtual Networks for production deployments
- Enable Azure Container Apps firewall
- Implement rate limiting for APIs
- Review Azure security baselines

## Pull Request Guidelines

### Before Submitting PR

```bash
# Format code
dotnet format

# Run all tests
dotnet test

# Ensure clean build
dotnet build --configuration Release
```

### PR Title Format

```
[Component/Scenario] Brief description

Examples:
[01-SemanticSearch] Add product filtering feature
[Products API] Fix vector search performance
[Documentation] Update setup instructions
```

### PR Requirements

- All tests must pass: `dotnet test`
- Code must be formatted: `dotnet format --verify-no-changes`
- Update relevant documentation (README files, XML comments)
- Add tests for new features
- Follow existing patterns and conventions
- Keep changes focused and minimal

### Review Process

- CLA must be signed (automated via bot)
- Security scans must pass (CodeQL, secret scanning)
- At least one reviewer approval required
- All CI checks must pass

## Additional Notes

### Common Commands Quick Reference

```bash
# Format code
dotnet format

# Build solution
dotnet build

# Run tests
dotnet test

# Run scenario
cd scenarios/<name>/src/eShopAppHost && dotnet run

# Deploy to Azure
cd scenarios/<name>/src/eShopAppHost && azd up

# Set secrets
dotnet user-secrets set "ConnectionStrings:openai" "<value>"
```

### Troubleshooting

**Issue:** "Connection string 'openai' is missing"
- **Solution:** Set user secrets as described in Configuration section

**Issue:** Docker/Podman not running
- **Solution:** Start Docker Desktop or Podman service before running `dotnet run`

**Issue:** Port conflicts
- **Solution:** Aspire automatically assigns ports. Check Aspire Dashboard for actual URLs

**Issue:** Azure deployment fails
- **Solution:** Ensure `az login` is authenticated and subscription has correct permissions

**Issue:** Tests failing after changes
- **Solution:** Run `dotnet restore` and `dotnet build` before `dotnet test`

### Performance Considerations

- Vector searches are compute-intensive; consider caching strategies
- Azure OpenAI has rate limits; implement retry logic for production
- Aspire Dashboard adds overhead; disable telemetry for production builds if needed
- SQL Server 2025 vector indexes improve performance for large datasets

### Useful Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure Developer CLI (azd) Documentation](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Generative AI for Beginners .NET](https://aka.ms/genainet)

### Quick Navigation Tips

- Use `find` or `grep` to locate files across scenarios:
  ```bash
  find . -name "*.csproj" | grep Products
  grep -r "SemanticSearch" --include="*.cs"
  ```
- Each scenario has a `docs/` folder with detailed technical documentation
- Check `copilot-instructions.md` for additional coding guidelines
- Review scenario READMEs for specific configuration requirements

### CI/CD Integration

- GitHub Actions workflows are in `.github/workflows/`
- CodeQL security scanning runs automatically on PRs
- Profanity filter and documentation link validation run on commits
- Welcome bot greets new contributors

### Development Tips

- Start with `01-SemanticSearch` scenario to understand the base architecture
- Each scenario builds on concepts from previous ones
- Aspire Dashboard provides real-time telemetry and logs
- Use Application Insights for production monitoring
- Test locally before deploying to Azure to save costs
