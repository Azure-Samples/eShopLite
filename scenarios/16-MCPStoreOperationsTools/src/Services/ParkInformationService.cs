using DataEntities;
using McpToolsEntities;
using Microsoft.Extensions.Logging;
using SearchEntities;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services;

public class ParkInformationService
{
    HttpClient httpClient;
    private readonly ILogger<WeatherService> _logger;

    public ParkInformationService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
		_logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<ParkInformationToolResponse?> GetParkInformation(string parkName)
    {
        try
        {
            // call the desired Endpoint
            HttpResponseMessage response = null;
            response = await httpClient.GetAsync($"/api/getparkinfo/{parkName}");

            _logger.LogInformation($"Http status code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ParkInformationToolResponse>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                _logger.LogError($"Internal Server Error: {response.Content}");
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
            _logger.LogError(ex, "Error during GetWeather");
            throw;
        }

        return new ParkInformationToolResponse { ParkName= parkName };
    }    
}
