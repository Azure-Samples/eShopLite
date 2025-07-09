# DeepSeek Documentation

## Overview
The 05-deepseek scenario demonstrates an advanced eCommerce application with DeepSeek-R1 AI integration, providing sophisticated reasoning capabilities alongside traditional semantic search using .NET Aspire and multiple AI models.

## Features

### Core Services
- **Aspire Orchestration**: Coordinated microservices with persistent data storage
- **DeepSeek-R1 Integration**: Advanced AI reasoning for complex product recommendations
- **Application Insights**: Comprehensive telemetry for multi-model AI operations

### Feature Documentation
- [Aspire Orchestration](aspire-orchestration.md) - Service coordination with persistent storage
- [DeepSeek Integration](deepseek-integration.md) - Advanced AI reasoning and multi-model strategy
- [Application Insights](application-insights.md) - Telemetry and AI performance monitoring

## Architecture
The scenario consists of three main components:
1. **SQL Server Database** - Persistent data storage with container lifetime management
2. **Products Service** - REST API with multi-model AI capabilities
3. **Store Web Application** - Blazor frontend with advanced AI features

## AI Capabilities
- **DeepSeek-R1 Reasoning**: Advanced logical analysis and mathematical problem solving
- **GPT-4.1 Mini**: Fast responses for simple queries and interactions
- **Semantic Search**: Vector embeddings for intelligent product discovery
- **Multi-Model Strategy**: Intelligent selection between AI models based on query complexity

## Advanced Features
- **Chain-of-Thought Reasoning**: Detailed explanation of AI recommendations
- **Complex Product Analysis**: Sophisticated feature comparisons and evaluations
- **Mathematical Calculations**: Enhanced pricing analysis and cost comparisons
- **Persistent Storage**: Data retention across application restarts

## Configuration Requirements
- Azure OpenAI Service with multiple model deployments
- DeepSeek AI Foundry resource for DeepSeek-R1 access
- Separate connection strings for different AI services
- User secrets for API keys and endpoints
- .NET 9 runtime environment
- Docker support for persistent SQL Server

## Screenshots

> **Note**: The screenshots below represent the expected user interface when running the scenario. Due to infrastructure requirements (Azure OpenAI, DeepSeek AI Foundry, SQL Server), actual screenshots would require a fully configured environment.

### Aspire Dashboard
*The Aspire Dashboard showing all services with persistent storage indicators*

### Products Listing
*The main products page with enhanced AI recommendation features*

### Advanced Search
*The search interface demonstrating DeepSeek-R1 reasoning capabilities*

## Getting Started
1. Set up Azure OpenAI Service with GPT-4.1 Mini and text-embedding models
2. Configure DeepSeek AI Foundry resource with DeepSeek-R1 model
3. Set up separate user secrets for both AI services
4. Run `dotnet run --project src/eShopAppHost/eShopAppHost.csproj`
5. Navigate to the Aspire Dashboard to monitor service health
6. Use the Store application to test advanced AI reasoning features

## Model Selection Strategy
The application intelligently chooses between AI models:
- **Simple queries**: GPT-4.1 Mini for fast responses
- **Complex analysis**: DeepSeek-R1 for detailed reasoning
- **Search operations**: Text embeddings for semantic understanding