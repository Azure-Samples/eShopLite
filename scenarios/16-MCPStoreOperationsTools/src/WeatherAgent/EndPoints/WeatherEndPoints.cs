using McpToolsEntities;
using System.Globalization;

namespace WeatherAgent.EndPoints;

public static class WeatherEndPoints
{
    public static void MapWeatherEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        routes.MapGet("/", () => $"Weather Agent - {DateTime.Now}").ExcludeFromDescription();

        group.MapGet("/getweather/{city}",
            async (string city,
            HttpClient httpClient,
            ILogger <Program> logger,
            IConfiguration config) =>
            {
                return await GetWeatherAsync(city, httpClient, logger, config);
            })
            .WithName("GetWeather")
            .Produces<OnlineSearchToolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    internal static async Task<WeatherToolResponse> GetWeatherAsync(string city, HttpClient httpClient, ILogger<Program> logger, IConfiguration config)
    {
        WeatherToolResponse response = new WeatherToolResponse
        {
            CityName = city,
            WeatherCondition = string.Empty
        };
        try
        {
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
            var geoResponse = await httpClient.GetFromJsonAsync<GeoResponse>(geoUrl);

            var (lat, lon) = GetFormatedCoordinates(geoResponse, city);

            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
            var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(weatherUrl);

            var weather = weatherResponse?.current_weather;

            response.WeatherCondition =  weather is null
                ? $"Could not retrieve weather data for {city}"
                : $"Current temperature in {city} is {weather.temperature}°C with wind speed {weather.windspeed} km/h.";
        }
        catch (Exception ex)
        {
            var sanitizedCity = city.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
            logger.LogError(ex, $"Error retrieving weather data for {sanitizedCity}");
            response.WeatherCondition = $"Error retrieving weather data: {ex.Message}";
        }
        return response;
    }

    private static (string lat, string lon) GetFormatedCoordinates(GeoResponse? geoResponse, string city)
    {
        if (geoResponse?.results == null || geoResponse.results.Length == 0)
            throw new ArgumentException($"Could not find location for {city}");

        var (lat, lon) = (geoResponse.results[0].latitude, geoResponse.results[0].longitude);
        return (
            lat.ToString(CultureInfo.InvariantCulture),
            lon.ToString(CultureInfo.InvariantCulture)
        );
    }

    public class GeoResponse
    {
        public GeoResult[] results { get; set; }
    }

    public class GeoResult
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class WeatherResponse
    {
        public CurrentWeather current_weather { get; set; }
    }

    public class CurrentWeather
    {
        public double temperature { get; set; }
        public double windspeed { get; set; }
    }
}
