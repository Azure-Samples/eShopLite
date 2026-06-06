using SearchEntities;
using DataEntities;
using System.Text.Json;

namespace Store.Services;

public class ProductService : IProductService
{
    HttpClient httpClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
		_logger = logger;
        this.httpClient = httpClient;
    }
    public async Task<List<Product>> GetProducts()
    {
        List<Product>? products = null;
		try
		{
	    	var response = await httpClient.GetAsync("/api/product");
	    	var responseText = await response.Content.ReadAsStringAsync();

			_logger.LogInformation($"Http status code: {response.StatusCode}");
    	    _logger.LogInformation($"Http response content: {responseText}");

		    if (response.IsSuccessStatusCode)
		    {
				var options = new JsonSerializerOptions
				{
		    		PropertyNameCaseInsensitive = true
				};

				products = await response.Content.ReadFromJsonAsync(ProductSerializerContext.Default.ListProduct);
	   		 }
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during GetProducts.");
		}

		return products ?? new List<Product>();
    }

    public async Task<SearchResponse?> SearchWithType(string searchTerm, SearchType searchType)
    {
        try
        {
            HttpResponseMessage response = searchType switch
            {
                SearchType.Standard => await httpClient.GetAsync($"/api/product/search/{searchTerm}"),
                SearchType.Semantic => await httpClient.GetAsync($"/api/aisearch/{searchTerm}"),
                SearchType.A2A => await httpClient.GetAsync($"/api/a2asearch/{searchTerm}"),
                _ => await httpClient.GetAsync($"/api/product/search/{searchTerm}")
            };

            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");
            _logger.LogInformation($"Http response content: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                if (searchType == SearchType.A2A)
                {
                    // For A2A search, we need to handle the A2A response format
                    var a2aResponse = await response.Content.ReadFromJsonAsync<A2ASearchResponse>();
                    if (a2aResponse != null)
                    {
                        // Convert A2A response to standard SearchResponse format for UI compatibility
                        return new SearchResponse
                        {
                            Response = a2aResponse.Response,
                            Products = a2aResponse.Products?.Select(p => new Product
                            {
                                Id = int.TryParse(p.ProductId, out var id) ? id : 0,
                                Name = p.Name,
                                Description = $"{p.Description} | Stock: {p.Stock} | Promotions: {p.Promotions.Count} | Reviews: {p.Insights.Count}",
                                Price = p.Price,
                                ImageUrl = p.ImageUrl
                            }).ToList() ?? new List<Product>()
                        };
                    }
                }
                else
                {
                    return await response.Content.ReadFromJsonAsync<SearchResponse>();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SearchWithType for {searchType}.", searchType);
        }

        return new SearchResponse { Response = "No response" };
    }

    public async Task<SearchResponse?> Search(string searchTerm, bool semanticSearch = false)
    {
        try
        {
            // call the desired Endpoint
            HttpResponseMessage response = null;
            if (semanticSearch)
            {
                // AI Search
                response = await httpClient.GetAsync($"/api/aisearch/{searchTerm}");
            }
            else
            {
                // standard search
                response = await httpClient.GetAsync($"/api/product/search/{searchTerm}");
            }

            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");
            _logger.LogInformation($"Http response content: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SearchResponse>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Search.");
        }

        return new SearchResponse { Response = "No response" };
    }    
}
