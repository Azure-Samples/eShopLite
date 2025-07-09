# SQL Server 2025 Documentation

## Overview
The 08-Sql2025 scenario demonstrates a cutting-edge eCommerce application leveraging SQL Server 2025's enhanced vector capabilities and AI integration using .NET Aspire orchestration.

## Features

### Core Services
- **Aspire Orchestration**: Coordinated microservices with SQL Server 2025
- **SQL Server 2025 Vector Features**: Native vector storage and processing capabilities
- **Azure OpenAI Integration**: AI models optimized for SQL Server 2025 vector operations

### Feature Documentation
- [Aspire Orchestration](aspire-orchestration.md) - Service coordination with SQL Server 2025
- [SQL Server 2025 Vector Features](sql2025-vector-features.md) - Enhanced database capabilities
- [Azure OpenAI Integration](azure-openai-integration.md) - AI models and vector optimization

## Architecture
The scenario consists of three main components:
1. **SQL Server 2025 Database** - Advanced vector storage with native AI capabilities
2. **Products Service** - REST API leveraging SQL Server 2025 vector features
3. **Store Web Application** - Blazor frontend with enhanced search capabilities

## SQL Server 2025 Capabilities
- **Native Vector Support**: Built-in vector data types and operations
- **Enhanced Indexing**: Optimized vector similarity search indexes
- **AI Integration**: Embedded machine learning model support
- **Performance Optimization**: Hardware-accelerated vector processing

## AI Features
- **Semantic Search**: Native vector similarity using SQL Server 2025
- **Text Embeddings**: High-quality embeddings with text-embedding-3-small
- **Chat Completion**: GPT-4.1 Mini for intelligent product recommendations
- **Database-Level AI**: Machine learning operations directly in SQL Server

## Vector Processing Benefits
- **Reduced Latency**: Vector operations performed in the database
- **Simplified Architecture**: Single data store for relational and vector data
- **Cost Efficiency**: No external vector database required
- **ACID Compliance**: Transactional consistency for vector operations

## Configuration Requirements
- SQL Server 2025 container support
- Azure OpenAI Service with latest models
- Compatible Docker runtime with sufficient resources
- User secrets for API keys and endpoints
- .NET 9 runtime environment

## Screenshots

> **Note**: The screenshots below represent the expected user interface when running the scenario. Due to infrastructure requirements (SQL Server 2025, Azure OpenAI), actual screenshots would require a fully configured environment.

### Aspire Dashboard
*The Aspire Dashboard showing SQL Server 2025 and associated services*

### Products Listing
*The main products page with SQL Server 2025 enhanced search features*

### Vector Search
*The search interface demonstrating SQL Server 2025 native vector capabilities*

## Getting Started
1. Ensure Docker supports SQL Server 2025 containers
2. Set up Azure OpenAI Service with required models
3. Configure user secrets for API keys
4. Run `dotnet run --project src/eShopAppHost/eShopAppHost.csproj`
5. Navigate to the Aspire Dashboard to monitor SQL Server 2025
6. Use the Store application to test native vector search

## Performance Advantages
- **Hardware Acceleration**: SQL Server 2025 optimizes vector operations
- **Memory Efficiency**: Reduced data movement between services
- **Query Optimization**: Native SQL vector functions
- **Concurrent Processing**: Parallel vector operations support

## Migration Benefits
For organizations upgrading from earlier scenarios:
- **Simplified Infrastructure**: Consolidate vector storage in SQL Server
- **Improved Performance**: Native vector processing vs. external services
- **Cost Reduction**: Fewer external dependencies
- **Enhanced Security**: Vector data remains within SQL Server boundaries