using DataEntities;
using McpToolsEntities;
using Microsoft.Extensions.Logging;
using SearchEntities;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services;

public class WeatherService
{
    HttpClient httpClient;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
		_logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<WeatherToolResponse?> GetWeather(string city)
    {
        try
        {
            // the city parameter must contain text like "Seattle, WA" or "Seattle", perform the necessary encoding to be used in the URL
            city = Uri.EscapeDataString(city);

            // call the desired Endpoint
            HttpResponseMessage response = null;
            response = await httpClient.GetAsync($"/api/getweather/{city}");

            _logger.LogInformation($"Http status code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherToolResponse>();
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

        return new WeatherToolResponse { CityName = city, WeatherCondition = "Not Defined" };
    }    
}
