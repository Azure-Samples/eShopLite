# 01-SemanticSearch - Documentation

## Overview

The 01-SemanticSearch scenario demonstrates a comprehensive eCommerce solution built with .NET Aspire that integrates Azure OpenAI services for advanced search capabilities. This scenario showcases both traditional keyword search and semantic search functionality using vector embeddings.

## Features Documentation

This scenario implements the following key features:

- [Azure OpenAI Integration](./azure-openai-integration.md) - GPT-4.1-mini chat and text-embedding-ada-002 embeddings
- [SQL Server Database](./sql-server-database.md) - Product data storage and management
- [Products Service](./products-service.md) - REST API for product operations with AI-powered search
- [Store Frontend](./store-frontend.md) - Blazor web application with search interface
- [Vector Entities](./vector-entities.md) - Product vector storage and similarity search
- [Service Defaults](./service-defaults.md) - .NET Aspire service configuration and telemetry

## Architecture

The solution follows a microservices architecture orchestrated by .NET Aspire:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Store (UI)    │───▶│  Products API   │───▶│   SQL Server    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │ Azure OpenAI    │
                       │ - GPT-4.1-mini  │
                       │ - Embeddings    │
                       └─────────────────┘
```

## Screenshots

### Aspire Dashboard
![Aspire Dashboard](./images/dashboard.jpg)

### Products Listing
![Products Listing](./images/products.jpg)

### Semantic Search
![Semantic Search](./images/search.jpg)

## Getting Started

1. Navigate to the scenario directory: `cd scenarios/01-SemanticSearch/src/eShopAppHost`
2. Run the application: `dotnet run`
3. Access the Aspire Dashboard using the login URL displayed in the console
4. Navigate to the Store application to test search functionality

## Configuration

The application uses Azure OpenAI services that can be configured via:
- User secrets for local development
- Azure resources when deployed to production
- Environment variables for deployment names and endpoints

See individual feature documentation for detailed configuration instructions.