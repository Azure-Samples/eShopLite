using McpToolsEntities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Services;

public class OnlineResearcherService
{
    HttpClient httpClient;
    private readonly ILogger<ProductService> _logger;

    public OnlineResearcherService(HttpClient httpClient, ILogger<ProductService> logger)
    {
		_logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<OnlineSearchToolResponse?> Search(string searchTerm)
    {
        try
        {
            // call the desired Endpoint
            HttpResponseMessage response = null;
            response = await httpClient.GetAsync($"/searchonline/{searchTerm}");

            //var responseText = await response.Content .ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                //OnlineSearchToolResponse toolResponse = await response.Content.ReadFromJsonAsync<OnlineSearchToolResponse>();
                OnlineSearchToolResponse toolResponse = await response.Content.ReadFromJsonAsync<OnlineSearchToolResponse>();
                return toolResponse;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception($"Internal Server Error: {response.Content}");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Not Found: {response.Content}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Search.");
            throw ex;
        }

        return new OnlineSearchToolResponse { SearchResults = "No response" };
    }    
}
