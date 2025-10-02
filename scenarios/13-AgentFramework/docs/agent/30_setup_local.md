# Shopping Assistant Agent - Local Setup Guide

This guide walks you through setting up the Shopping Assistant Agent scenario on your local development machine.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

1. **.NET 8 SDK or later**
   - Download from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
   - Verify installation: `dotnet --version`

2. **Docker Desktop** or **Podman**
   - Docker Desktop: [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
   - Podman: [https://podman.io/](https://podman.io/)
   - Required for running SQL Server container

3. **.NET Aspire Workload**
   ```bash
   dotnet workload install aspire
   ```

4. **Git**
   - Download from [https://git-scm.com/downloads](https://git-scm.com/downloads)

### Recommended Tools

- **Visual Studio 2022** (17.8 or later) or **Visual Studio Code** with C# Dev Kit
- **Azure Developer CLI (azd)** - For Azure deployment
  ```bash
  # Windows (PowerShell)
  winget install microsoft.azd
  
  # macOS
  brew tap azure/azd && brew install azd
  
  # Linux
  curl -fsSL https://aka.ms/install-azd.sh | bash
  ```

### Azure Resources

You'll need access to:

- **Azure OpenAI Service** with:
  - `gpt-4o-mini` deployment
  - `text-embedding-ada-002` deployment
- **Azure Subscription** with appropriate permissions

## Step 1: Clone the Repository

```bash
git clone https://github.com/Azure-Samples/eShopLite.git
cd eShopLite/scenarios/13-AgentFramework
```

## Step 2: Configure Azure OpenAI

### Option A: Using Existing Azure OpenAI Resource

If you have an existing Azure OpenAI resource:

1. Navigate to the ShoppingAssistantAgent project:
   ```bash
   cd src/ShoppingAssistantAgent
   ```

2. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

3. Set your Azure OpenAI configuration:
   ```bash
   dotnet user-secrets set "OpenAI:Endpoint" "https://your-resource.openai.azure.com/"
   dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
   dotnet user-secrets set "OpenAI:DeploymentName" "gpt-4o-mini"
   ```

4. Also configure the Products API:
   ```bash
   cd ../Products
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-resource.openai.azure.com/;Key=your-api-key-here;"
   dotnet user-secrets set "AI_ChatDeploymentName" "gpt-4o-mini"
   dotnet user-secrets set "AI_embeddingsDeploymentName" "text-embedding-ada-002"
   ```

### Option B: Let Aspire Create Resources

If you don't have Azure OpenAI resources, Aspire can create them for you:

1. Ensure you're logged into Azure:
   ```bash
   azd auth login
   ```

2. Skip the secrets configuration - Aspire will provision resources on first run

## Step 3: Build the Solution

Navigate to the src directory and build:

```bash
cd src
dotnet restore
dotnet build
```

Verify that the build completes without errors.

## Step 4: Start the Application

### Using .NET Aspire AppHost

1. Navigate to the AppHost project:
   ```bash
   cd eShopAppHost
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The Aspire Dashboard will open automatically in your browser, showing:
   - All running services
   - Service endpoints
   - Logs and traces
   - Resource status

### Using Visual Studio

1. Open `src/eShopLite-AgentFramework.sln` in Visual Studio

2. Set `eShopAppHost` as the startup project

3. Press F5 to run

### Using Visual Studio Code

1. Open the `scenarios/13-AgentFramework` folder in VS Code

2. Open the Command Palette (Ctrl+Shift+P / Cmd+Shift+P)

3. Select `.NET: Run Aspire App`

4. Choose `eShopAppHost`

## Step 5: Access the Application

Once the application starts, you'll see output similar to:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Access Points

- **Store UI**: `https://localhost:7001`
  - Main eCommerce application
  - Shopping Assistant chat interface

- **Aspire Dashboard**: `https://localhost:17001`
  - Service management
  - Logs and traces
  - Resource monitoring

- **Shopping Assistant Agent**: `https://localhost:7002`
  - Swagger UI at `/swagger`
  - Chat API at `/api/agent/chat`

- **Products API**: `https://localhost:7003`
  - Swagger UI at `/swagger`
  - Product endpoints

## Step 6: Test the Shopping Assistant

1. Open the Store UI in your browser

2. Look for the chat icon (usually in the top-right or bottom-right corner)

3. Click to open the chat panel

4. Try these sample queries:
   - "Show me hiking boots"
   - "Tell me more about product 1"
   - "Add that to my cart"
   - "What outdoor gear do you have?"

## Troubleshooting

### Issue: Port Already in Use

**Solution:** Change ports in `eShopAppHost/Properties/launchSettings.json`

### Issue: SQL Server Container Won't Start

**Solution:** 
1. Ensure Docker Desktop is running
2. Check available ports: `netstat -an | findstr :1433`
3. Try restarting Docker Desktop

### Issue: Azure OpenAI Rate Limits

**Solution:**
1. Check your quota in Azure Portal
2. Reduce concurrent requests
3. Consider upgrading to higher quota

### Issue: Agent Not Responding

**Solution:**
1. Check Aspire Dashboard for errors
2. Verify OpenAI configuration in user secrets
3. Check network connectivity to Azure OpenAI
4. Review logs in Application Insights

### Issue: Build Errors

**Solution:**
1. Clean solution: `dotnet clean`
2. Remove bin/obj folders: `rm -rf **/bin **/obj`
3. Restore packages: `dotnet restore`
4. Rebuild: `dotnet build`

## Verification Steps

After setup, verify:

1. ✅ All services show "Running" in Aspire Dashboard
2. ✅ Store UI loads without errors
3. ✅ Chat panel opens and accepts input
4. ✅ Agent responds to simple queries
5. ✅ Products appear in search results
6. ✅ Add to cart functionality works

## Next Steps

- Review the [Architecture Documentation](20_architecture.md)
- Read the [User Guide](50_user_guide.md) for usage tips
- See [Admin Guide](60_admin_guide.md) for configuration options
- Try deploying to Azure using [Azure Setup Guide](40_setup_azure.md)

## Additional Configuration

### Enable Detailed Logging

Add to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ShoppingAssistantAgent": "Debug"
    }
  }
}
```

### Configure CORS for Local Testing

In `ShoppingAssistantAgent/Program.cs`, CORS is already configured for local development.

### Customize Agent Behavior

Edit tool descriptions in:
- `Tools/SearchCatalogTool.cs`
- `Tools/ProductDetailsTool.cs`
- `Tools/AddToCartTool.cs`

## Development Workflow

1. Make code changes
2. Aspire will automatically rebuild and restart affected services
3. Refresh browser to see changes
4. Check Aspire Dashboard for logs and errors
5. Iterate

## Performance Tips

- Use `dotnet build` instead of `dotnet run --no-build` when iterating
- Keep Docker Desktop resources allocated appropriately
- Monitor token usage in Azure OpenAI to manage costs
- Use local caching for frequently accessed data

## Getting Help

If you encounter issues:

1. Check the [Troubleshooting](#troubleshooting) section above
2. Review logs in Aspire Dashboard
3. Search existing GitHub issues
4. Create a new issue with:
   - Steps to reproduce
   - Error messages
   - Environment details
   - Aspire Dashboard screenshots
