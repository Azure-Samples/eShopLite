using SearchEntities;
using DataEntities;
using System.Text.Json;

namespace Store.Services;

public class ProductService : IProductService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IObservabilityService _observabilityService;

    public ProductService(
        HttpClient httpClient,
        ILogger<ProductService> logger,
        IObservabilityService observabilityService)
    {
		_logger = logger;
        this.httpClient = httpClient;
        _observabilityService = observabilityService;
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

    public async Task<SearchResponse?> Search(
        string searchTerm,
        bool semanticSearch = false,
        bool injectFailure = false)
    {
        var searchKind = semanticSearch ? "semantic" : "keyword";

        // Emit a telemetry event for every search so the Observability Assistant
        // has real data to analyze in the selected time window.
        await _observabilityService.IngestEventAsync(
            "store",
            "Information",
            $"User ran a {searchKind} search for '{searchTerm}' (injectFailure={injectFailure.ToString().ToLowerInvariant()}).");

        try
        {
            // call the desired Endpoint
            HttpResponseMessage response = null;
            var encodedSearchTerm = Uri.EscapeDataString(searchTerm ?? string.Empty);
            var querySuffix = $"?injectFailure={injectFailure.ToString().ToLowerInvariant()}";
            if (semanticSearch)
            {
                // AI Search
                response = await httpClient.GetAsync($"/api/aisearch/{encodedSearchTerm}{querySuffix}");
            }
            else
            {
                // standard search
                response = await httpClient.GetAsync($"/api/product/search/{encodedSearchTerm}{querySuffix}");
            }

            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");
            _logger.LogInformation($"Http response content: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                await _observabilityService.IngestEventAsync(
                    "products",
                    "Information",
                    $"Products {searchKind} search for '{searchTerm}' completed successfully (HTTP {(int)response.StatusCode}).");

                return await response.Content.ReadFromJsonAsync<SearchResponse>();
            }

            await _observabilityService.IngestEventAsync(
                "products",
                "Error",
                $"Products {searchKind} search for '{searchTerm}' failed with HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Search.");

            await _observabilityService.IngestEventAsync(
                "products",
                "Error",
                $"Products {searchKind} search for '{searchTerm}' threw {ex.GetType().Name}: {ex.Message}");
        }

        return new SearchResponse { Response = "No response" };
    }
}
