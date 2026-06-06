using McpToolsEntities;
using System.Globalization;

namespace ParkInformationAgent.EndPoints;

public static class ParkInformationAgentEndPoints
{
    public static void MapParkInformationEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        routes.MapGet("/", () => $"Park Information Agent - {DateTime.Now}").ExcludeFromDescription();

        group.MapGet("/getparkinfo/{parkName}",
            async (string parkName,
            ILogger <Program> logger,
            IConfiguration config) =>
            {
                return await GetParkInformationAsync(parkName, logger, config);
            })
            .WithName("GetParkInfo")
            .Produces<ParkInformationToolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    internal static async Task<ParkInformationToolResponse> GetParkInformationAsync(
        string parkName, ILogger<Program> logger, IConfiguration config)
    {
        ParkInformationToolResponse response = new()
        {
            ParkName = parkName
        };

        // generate random park information
        var random = new Random();
        var parkNames = new List<string> { "Yellowstone", "Yosemite", "Grand Canyon", "Zion", "Rocky Mountain" };
        var parkDescriptions = new List<string>
        {
            "A beautiful national park with stunning landscapes.",
            "Famous for its waterfalls and giant sequoias.",
            "Known for its breathtaking canyon views.",
            "A paradise for hikers and nature lovers.",
            "Home to majestic mountains and diverse wildlife."
        };
        var openingHours = new List<string>
        {
            "8:00 AM - 6:00 PM",
            "7:00 AM - 7:00 PM",
            "9:00 AM - 5:00 PM",
            "10:00 AM - 4:00 PM"
        };
        var locations = new List<string>
        {
            "California, USA",
            "Wyoming, USA",
            "Arizona, USA",
            "Utah, USA",
            "Colorado, USA"
        };
        var transportationTypes = new List<string>
        {
            "Car",
            "Bicycle",
            "Hiking",
            "Public Transport"
        };
        var facilities = new List<string>
        {
            "Restrooms",
            "Visitor Center",
            "Camping Sites",
            "Picnic Areas"
        };
        var randomParkName = parkNames[random.Next(parkNames.Count)];
        var randomParkDescription = parkDescriptions[random.Next(parkDescriptions.Count)];
        var randomOpeningHours = openingHours[random.Next(openingHours.Count)];
        var randomLocation = locations[random.Next(locations.Count)];
        var randomTransportationType = transportationTypes[random.Next(transportationTypes.Count)];
        var randomFacilities = facilities[random.Next(facilities.Count)];
        //response.ParkName = randomParkName;

        response.ParkDescription = randomParkDescription;
        response.OpeningHours = randomOpeningHours;
        response.Location = randomLocation;
        response.TransportationType = randomTransportationType;
        response.Facilities = randomFacilities;
        response.ParkInformation = $"Park Name: {randomParkName}, Description: {randomParkDescription}, Opening Hours: {randomOpeningHours}, Location: {randomLocation}, Transportation Type: {randomTransportationType}, Facilities: {randomFacilities}";
        logger.LogInformation($"Park Information: {response.ParkInformation}");


        return response;
    }

}
