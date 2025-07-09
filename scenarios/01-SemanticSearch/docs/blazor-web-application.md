# Blazor Web Application

## Overview
The Store service provides a modern Blazor Server web application with interactive components for product browsing and search functionality.

## Application Configuration

### Service Registration
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));
```
- **Purpose**: Blazor Server with interactive components and HTTP client for API communication
- **Configuration**: Service discovery for Products API communication
- **Rendering**: Server-side rendering with interactive server components

### HTTP Client Configuration
```csharp
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));
```
- **Purpose**: HTTP client configured for Products service communication
- **Configuration**: Base address uses Aspire service discovery ("products" service name)
- **Protocol**: Supports both HTTP and HTTPS

## Key Components

### Search Page (`/search`)
```csharp
@page "/search"
@inject Store.Services.ProductService ProductService
@attribute [StreamRendering(true)]
@rendermode InteractiveServer
```

#### Search UI Features
- Text input for search queries
- Toggle switch for semantic vs keyword search
- Results table with product images, names, descriptions, and prices
- AI-generated response display

#### Search Implementation
```csharp
private async Task DoSearch()
{
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var response = await ProductService.Search(searchTerm, smartSearch);
        products = response?.Products ?? new List<Product>();
        aiResponse = response?.Response ?? "";
    }
}
```

### ProductService
```csharp
public async Task<SearchResponse?> Search(string searchTerm, bool semanticSearch = false)
{
    HttpResponseMessage response = null;
    if (semanticSearch)
    {
        response = await httpClient.GetAsync($"/api/aisearch/{searchTerm}");
    }
    else
    {
        response = await httpClient.GetAsync($"/api/product/search/{searchTerm}");
    }
    return await response.Content.ReadFromJsonAsync<SearchResponse>();
}
```

## Pages Structure

### Home Page (`/`)
- Landing page with navigation to products and search

### Products Page (`/products`) 
- Displays all available products in a table format
- Product images hosted on external CDN
- Direct communication with Products API

### Search Page (`/search`)
- Advanced search interface with semantic/keyword toggle
- Displays search results and AI-generated responses
- Interactive server-side rendering

## Configuration Notes
- **Rendering Mode**: InteractiveServer for real-time UI updates
- **Stream Rendering**: Enabled for progressive loading
- **Service Discovery**: Automatic Products service resolution via Aspire
- **Image CDN**: External GitHub repository for product images
- **Error Handling**: Graceful fallbacks for failed API calls

## External Dependencies
- Products service API
- External CDN for product images (GitHub raw content)
- Microsoft.AspNetCore.Components.Web