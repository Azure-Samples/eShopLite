using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using SearchEntities;
using DataEntities;

var builder = WebApplication.CreateBuilder(args);

// Add default services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// add product service for API calls
builder.Services.AddSingleton<ProductApiService>();
builder.Services.AddHttpClient<ProductApiService>(
    static client => client.BaseAddress = new("https+http://products"));

// add MCP server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();

// Initialize default endpoints
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// map endpoints
app.MapGet("/", () => $"eShopLite MCP App - Product Search! {DateTime.Now}");
app.MapMcp("/mcp");

Console.WriteLine();
Console.WriteLine("🛍️ eShopLite MCP App - Product Search");
Console.WriteLine("======================================");
Console.WriteLine("MCP server listening on http://localhost:5200/mcp");
Console.WriteLine();
Console.WriteLine("Add to your VS Code MCP config:");
Console.WriteLine("  \"url\": \"http://localhost:5200/mcp\"");
Console.WriteLine("  \"type\": \"http\"");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop the server");
Console.WriteLine();

await app.RunAsync();

/// <summary>
/// Product API Service for calling the Products API
/// </summary>
public class ProductApiService(HttpClient httpClient)
{
    public async Task<SearchResponse> SemanticSearchAsync(string query)
    {
        var response = await httpClient.GetAsync($"/api/aisearch/{Uri.EscapeDataString(query)}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
        return result ?? new SearchResponse();
    }

    public async Task<SearchResponse> KeywordSearchAsync(string query)
    {
        var response = await httpClient.GetAsync($"/api/Product/search/{Uri.EscapeDataString(query)}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
        return result ?? new SearchResponse();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var response = await httpClient.GetAsync("/api/Product");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Product>>();
        return result ?? [];
    }
}

/// <summary>
/// HTML content provider for the product search UI
/// </summary>
public static class ProductSearchHtmlProvider
{
    public static Task<string> GetHtml()
    {
        var html = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>eShopLite Product Search</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      padding: 16px;
      min-height: 100vh;
      background: var(--vscode-editor-background, #1e1e1e);
      color: var(--vscode-editor-foreground, #cccccc);
    }
    .container { max-width: 900px; margin: 0 auto; }
    h1 { font-size: 20px; margin-bottom: 16px; text-align: center; display: flex; align-items: center; justify-content: center; gap: 8px; }
    .search-row { display: flex; gap: 8px; margin-bottom: 16px; }
    .search-input {
      flex: 1; padding: 12px 16px;
      border: 1px solid var(--vscode-input-border, #3c3c3c);
      border-radius: 6px;
      background: var(--vscode-input-background, #3c3c3c);
      color: var(--vscode-input-foreground, #cccccc);
      font-size: 14px;
    }
    .search-input:focus {
      outline: none;
      border-color: var(--vscode-focusBorder, #007fd4);
    }
    .search-btn {
      padding: 12px 24px; border: none; border-radius: 6px;
      background: var(--vscode-button-background, #0e639c);
      color: var(--vscode-button-foreground, #ffffff);
      cursor: pointer; font-size: 14px; font-weight: 500;
      transition: background 0.2s;
    }
    .search-btn:hover {
      background: var(--vscode-button-hoverBackground, #1177bb);
    }
    .search-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .search-options {
      display: flex; gap: 16px; margin-bottom: 16px;
      justify-content: center;
    }
    .search-option {
      display: flex; align-items: center; gap: 6px;
      cursor: pointer; font-size: 13px;
    }
    .search-option input[type="radio"] {
      accent-color: var(--vscode-button-background, #0e639c);
    }
    .response-box {
      background: var(--vscode-textBlockQuote-background, #2d2d2d);
      border: 1px solid var(--vscode-input-border, #3c3c3c);
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 16px;
      font-size: 14px;
      line-height: 1.6;
      white-space: pre-wrap;
      display: none;
    }
    .response-box.visible {
      display: block;
    }
    .products-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }
    .product-card {
      background: var(--vscode-editor-background, #1e1e1e);
      border: 1px solid var(--vscode-input-border, #3c3c3c);
      border-radius: 8px;
      overflow: hidden;
      transition: transform 0.2s, box-shadow 0.2s;
    }
    .product-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
      border-color: var(--vscode-focusBorder, #007fd4);
    }
    .product-image {
      width: 100%;
      height: 150px;
      object-fit: cover;
      background: var(--vscode-textBlockQuote-background, #2d2d2d);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 48px;
    }
    .product-info {
      padding: 12px;
    }
    .product-name {
      font-weight: 600;
      font-size: 14px;
      margin-bottom: 4px;
      color: var(--vscode-textLink-foreground, #3794ff);
    }
    .product-description {
      font-size: 12px;
      color: var(--vscode-descriptionForeground, #888);
      margin-bottom: 8px;
      line-height: 1.4;
    }
    .product-price {
      font-weight: 700;
      font-size: 16px;
      color: var(--vscode-terminal-ansiGreen, #4ec94e);
    }
    .status {
      text-align: center; font-size: 13px; margin-top: 12px; min-height: 20px;
      color: var(--vscode-descriptionForeground, #888);
    }
    .status.success { color: var(--vscode-terminal-ansiGreen, #4ec94e); }
    .status.error { color: var(--vscode-terminal-ansiRed, #f14c4c); }
    .loading {
      display: none;
      text-align: center;
      padding: 24px;
    }
    .loading.visible {
      display: block;
    }
    .spinner {
      display: inline-block;
      width: 24px;
      height: 24px;
      border: 3px solid var(--vscode-input-border, #3c3c3c);
      border-top-color: var(--vscode-button-background, #0e639c);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .empty-state {
      text-align: center;
      padding: 48px 24px;
      color: var(--vscode-descriptionForeground, #888);
    }
    .empty-state-icon {
      font-size: 48px;
      margin-bottom: 12px;
    }
    .product-count {
      font-size: 12px;
      color: var(--vscode-descriptionForeground, #888);
      margin-bottom: 12px;
    }
  </style>
</head>
<body>
  <div class="container">
    <h1>🛍️ eShopLite Product Search</h1>
    
    <div class="search-row">
      <input type="text" id="search-input" class="search-input" placeholder="Search for outdoor products..." />
      <button class="search-btn" id="search-btn">🔍 Search</button>
    </div>
    
    <div class="search-options">
      <label class="search-option">
        <input type="radio" name="search-type" value="semantic" checked />
        <span>🧠 Semantic Search (AI-powered)</span>
      </label>
      <label class="search-option">
        <input type="radio" name="search-type" value="keyword" />
        <span>🔤 Keyword Search</span>
      </label>
    </div>
    
    <div class="response-box" id="response-box"></div>
    
    <div class="loading" id="loading">
      <div class="spinner"></div>
      <p style="margin-top: 12px;">Searching products...</p>
    </div>
    
    <div class="product-count" id="product-count"></div>
    <div class="products-grid" id="products-grid"></div>
    
    <div class="empty-state" id="empty-state">
      <div class="empty-state-icon">🏕️</div>
      <p>Enter a search query to find outdoor products</p>
      <p style="margin-top: 8px; font-size: 12px;">Try: "camping gear", "hiking equipment", "flashlight"</p>
    </div>
    
    <div class="status" id="status"></div>
  </div>
  
  <script type="module">
    const searchInput = document.getElementById('search-input');
    const searchBtn = document.getElementById('search-btn');
    const responseBox = document.getElementById('response-box');
    const productsGrid = document.getElementById('products-grid');
    const loading = document.getElementById('loading');
    const emptyState = document.getElementById('empty-state');
    const status = document.getElementById('status');
    const productCount = document.getElementById('product-count');
    
    function getSearchType() {
      return document.querySelector('input[name="search-type"]:checked').value;
    }
    
    function getProductEmoji(name) {
      const lower = name.toLowerCase();
      if (lower.includes('flashlight') || lower.includes('lantern')) return '🔦';
      if (lower.includes('tent')) return '⛺';
      if (lower.includes('backpack')) return '🎒';
      if (lower.includes('jacket') || lower.includes('rain')) return '🧥';
      if (lower.includes('stove') || lower.includes('cookware')) return '🍳';
      if (lower.includes('pole')) return '🥢';
      if (lower.includes('kit')) return '🧰';
      return '🏕️';
    }
    
    function renderProducts(products) {
      productsGrid.innerHTML = '';
      
      if (!products || products.length === 0) {
        productCount.textContent = '';
        return;
      }
      
      productCount.textContent = `Found ${products.length} product${products.length > 1 ? 's' : ''}`;
      
      products.forEach(product => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.innerHTML = `
          <div class="product-image">${getProductEmoji(product.name)}</div>
          <div class="product-info">
            <div class="product-name">${product.name}</div>
            <div class="product-description">${product.description}</div>
            <div class="product-price">$${product.price.toFixed(2)}</div>
          </div>
        `;
        productsGrid.appendChild(card);
      });
    }
    
    async function doSearch() {
      const query = searchInput.value.trim();
      if (!query) {
        status.textContent = 'Please enter a search query';
        status.className = 'status error';
        return;
      }
      
      const searchType = getSearchType();
      
      // Show loading
      loading.classList.add('visible');
      emptyState.style.display = 'none';
      responseBox.classList.remove('visible');
      productsGrid.innerHTML = '';
      productCount.textContent = '';
      searchBtn.disabled = true;
      status.textContent = '';
      
      try {
        // Send search request to host
        const message = {
          type: 'mcp-app-search',
          query: query,
          searchType: searchType
        };
        
        if (window.parent !== window) {
          window.parent.postMessage(message, '*');
        }
        
        // For demo purposes, show a message that the search was triggered
        status.textContent = `${searchType === 'semantic' ? '🧠 Semantic' : '🔤 Keyword'} search triggered for: "${query}"`;
        status.className = 'status success';
        
      } catch (error) {
        status.textContent = `Error: ${error.message}`;
        status.className = 'status error';
      } finally {
        loading.classList.remove('visible');
        searchBtn.disabled = false;
      }
    }
    
    // Event listeners
    searchBtn.addEventListener('click', doSearch);
    searchInput.addEventListener('keypress', (e) => {
      if (e.key === 'Enter') doSearch();
    });
    
    // Listen for search results from host
    window.addEventListener('message', (event) => {
      if (event.data?.type === 'mcp-app-search-results') {
        loading.classList.remove('visible');
        searchBtn.disabled = false;
        
        if (event.data.error) {
          status.textContent = `Error: ${event.data.error}`;
          status.className = 'status error';
          return;
        }
        
        if (event.data.response) {
          responseBox.textContent = event.data.response;
          responseBox.classList.add('visible');
        }
        
        if (event.data.products) {
          renderProducts(event.data.products);
          emptyState.style.display = 'none';
        }
        
        status.textContent = `✓ Search completed`;
        status.className = 'status success';
      }
      
      if (event.data?.initialQuery) {
        searchInput.value = event.data.initialQuery;
        doSearch();
      }
    });
  </script>
</body>
</html>
""";
        return Task.FromResult(html);
    }
}

/// <summary>
/// Product Search MCP Tools
/// </summary>
[McpServerToolType]
public static class ProductSearchTools
{
    /// <summary>
    /// Opens an interactive product search UI.
    /// </summary>
    [McpServerTool]
    [Description("Open an interactive product search UI to find outdoor camping products. The UI allows users to search products using semantic AI-powered search or keyword search.")]
    [McpMeta("ui", JsonValue = """{ "resourceUri": "ui://product-search/app.html" }""")]
    public static ProductSearchResult ProductSearch(
        [Description("Initial search query (optional). Leave empty to open the search UI without a pre-filled query.")]
        string? query = "")
    {
        return new ProductSearchResult
        {
            Query = query ?? "",
            Message = "Opening product search UI..."
        };
    }

    /// <summary>
    /// Performs a semantic search for products.
    /// </summary>
    [McpServerTool(Name = "SemanticSearchProducts")]
    [Description("Performs a semantic AI-powered search in the outdoor products catalog. Returns matching products with an AI-generated response. Use this for natural language queries like 'camping gear for beginners' or 'waterproof equipment'.")]
    public static async Task<ProductsSearchToolResponse> SemanticSearchProducts(
        ProductApiService productApiService,
        ILogger<ProductApiService> logger,
        [Description("The search query to be used in the semantic products search")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"Function Semantic Search products: {query}");

        SearchResponse response = new();
        try
        {
            response = await productApiService.SemanticSearchAsync(query);
            response.McpFunctionCallName = "SemanticSearchProducts";
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Semantic Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        logger.LogInformation($"Response: {response?.Response}");
        logger.LogInformation("==========================");
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }

    /// <summary>
    /// Performs a keyword search for products.
    /// </summary>
    [McpServerTool(Name = "KeywordSearchProducts")]
    [Description("Searches products in the database by matching the query string with product names. Use this for specific product name searches like 'flashlight' or 'tent'.")]
    public static async Task<ProductsSearchToolResponse> KeywordSearchProducts(
        ProductApiService productApiService,
        ILogger<ProductApiService> logger,
        [Description("The search query to be used in the keyword products search")] string query)
    {
        logger.LogInformation("==========================");
        logger.LogInformation($"Function Keyword Search products: {query}");

        SearchResponse response = new();
        try
        {
            response = await productApiService.KeywordSearchAsync(query);
            response.McpFunctionCallName = "KeywordSearchProducts";
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during Keyword Search: {ex.Message}");
            response.Response = $"No response. {ex}";
        }

        logger.LogInformation($"Response: {response?.Response}");
        logger.LogInformation("==========================");
        return new ProductsSearchToolResponse()
        {
            SearchResponse = response
        };
    }
}

/// <summary>
/// Product Search MCP Resources
/// </summary>
[McpServerResourceType]
public static class ProductSearchResources
{
    /// <summary>
    /// Provides the HTML UI for the product search app
    /// </summary>
    [McpServerResource(
        UriTemplate = "ui://product-search/app.html",
        MimeType = "text/html",
        Title = "Product Search UI")]
    [Description("Interactive product search UI for eShopLite")]
    public static async Task<string> GetProductSearchUI()
    {
        return await ProductSearchHtmlProvider.GetHtml();
    }
}

public class ProductSearchResult
{
    public string Query { get; set; } = "";
    public string Message { get; set; } = "";

    public override string ToString() =>
        string.IsNullOrEmpty(Query)
            ? "Product Search UI ready."
            : $"Product Search UI ready with query: {Query}";
}

public class ProductsSearchToolResponse
{
    public SearchResponse? SearchResponse { get; set; }

    public override string ToString()
    {
        if (SearchResponse == null) return "No results";
        var productCount = SearchResponse.Products?.Count ?? 0;
        return $"Found {productCount} products. {SearchResponse.Response}";
    }
}
