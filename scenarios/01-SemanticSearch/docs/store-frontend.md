# Store Frontend

## Overview

The Store Frontend is a Blazor Server application that provides the user interface for the eCommerce platform. It offers both traditional keyword search and AI-powered semantic search capabilities through an intuitive web interface.

## Key Features

### User Interface Components
- **Product Catalog**: Grid display of available products with images and details
- **Search Interface**: Dual search modes (keyword and semantic)
- **Product Details**: Individual product pages with comprehensive information
- **Search Results**: Formatted display of search results with relevance scoring

### Search Capabilities
- **Keyword Search**: Traditional text-based product search
- **Semantic Search**: AI-powered natural language search
- **Search Filters**: Category, price range, and feature filtering
- **Auto-complete**: Real-time search suggestions

## Technical Implementation

### Blazor Server Configuration
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Product service integration
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

### Service Integration
```csharp
// Products API client
public class ProductService
{
    private readonly HttpClient _httpClient;
    
    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Product>>("/products");
    }
    
    public async Task<List<ProductSearchResult>> SearchAsync(string query)
    {
        return await _httpClient.GetFromJsonAsync<List<ProductSearchResult>>($"/ai-search?search={query}");
    }
}
```

## Core Components

### Product Catalog Page
```razor
@page "/"
@inject ProductService ProductService

<PageTitle>eShop Lite - Products</PageTitle>

<div class="products-grid">
    @if (products != null)
    {
        @foreach (var product in products)
        {
            <div class="product-card">
                <img src="@product.ImageUrl" alt="@product.Name" />
                <h3>@product.Name</h3>
                <p>@product.Description</p>
                <span class="price">$@product.Price</span>
            </div>
        }
    }
</div>

@code {
    private List<Product> products;
    
    protected override async Task OnInitializedAsync()
    {
        products = await ProductService.GetProductsAsync();
    }
}
```

### Search Component
```razor
@page "/search"
@inject ProductService ProductService

<div class="search-container">
    <div class="search-input">
        <input @bind="searchQuery" @onkeypress="OnKeyPress" placeholder="Search products..." />
        <button @onclick="PerformSearch">Search</button>
    </div>
    
    <div class="search-mode">
        <label>
            <input type="radio" @bind="isSemanticSearch" value="false" /> Keyword Search
        </label>
        <label>
            <input type="radio" @bind="isSemanticSearch" value="true" /> AI Semantic Search
        </label>
    </div>
</div>

<div class="search-results">
    @if (searchResults != null)
    {
        @foreach (var result in searchResults)
        {
            <div class="search-result">
                <h4>@result.Name</h4>
                <p>@result.Description</p>
                <span class="relevance">Relevance: @result.Score.ToString("F2")</span>
            </div>
        }
    }
</div>

@code {
    private string searchQuery = "";
    private bool isSemanticSearch = true;
    private List<ProductSearchResult> searchResults;
    
    private async Task PerformSearch()
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            if (isSemanticSearch)
            {
                searchResults = await ProductService.SearchAsync(searchQuery);
            }
            else
            {
                // Keyword search implementation
            }
        }
    }
}
```

## Styling and User Experience

### CSS Framework
The application uses modern CSS with responsive design:
```css
.products-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1rem;
    padding: 1rem;
}

.product-card {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 1rem;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    transition: transform 0.2s;
}

.product-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.search-container {
    background: #f8f9fa;
    padding: 1rem;
    border-radius: 8px;
    margin-bottom: 1rem;
}

.search-input {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1rem;
}

.search-input input {
    flex: 1;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
}
```

### Responsive Design
- **Mobile-first**: Optimized for mobile devices
- **Adaptive Grid**: Product grid adjusts to screen size
- **Touch-friendly**: Appropriate touch targets for mobile
- **Performance**: Optimized images and lazy loading

## State Management

### Component State
```csharp
// Search state management
private SearchState currentSearch = new();

public class SearchState
{
    public string Query { get; set; } = "";
    public bool IsSemanticSearch { get; set; } = true;
    public List<ProductSearchResult> Results { get; set; } = new();
    public bool IsLoading { get; set; } = false;
}
```

### Navigation
- **Client-side routing**: Fast navigation between pages
- **Browser history**: Proper back/forward button support
- **Deep linking**: Direct links to search results

## Performance Optimization

### Blazor Server Optimizations
- **SignalR connection**: Efficient real-time updates
- **Component rendering**: Optimized render cycles
- **State preservation**: Maintain state across interactions
- **Memory management**: Proper component disposal

### Caching Strategies
- **API response caching**: Cache product data
- **Image optimization**: Compressed and cached images
- **Static asset caching**: CSS and JavaScript caching

## Error Handling

### User-Friendly Errors
```razor
@if (hasError)
{
    <div class="alert alert-danger">
        <h4>Oops! Something went wrong</h4>
        <p>We're having trouble loading the products. Please try again.</p>
        <button @onclick="RetryLoad">Retry</button>
    </div>
}
```

### Graceful Degradation
- **Offline capability**: Basic functionality when offline
- **Fallback search**: Keyword search when AI unavailable
- **Loading states**: Clear feedback during operations
- **Timeout handling**: Reasonable timeout periods