using System.Text.Json.Serialization;

namespace DataEntities;

public class Location
{
    public Location()
    {
        Id = 0;
        Name = "not defined";
        Address = "not defined";
        City = "not defined";
        State = "not defined";
        Country = "not defined";
        PostalCode = "not defined";
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("name")]
    public virtual string Name { get; set; }

    [JsonPropertyName("address")]
    public virtual string Address { get; set; }

    [JsonPropertyName("city")]
    public virtual string City { get; set; }

    [JsonPropertyName("state")]
    public virtual string State { get; set; }

    [JsonPropertyName("country")]
    public virtual string Country { get; set; }

    [JsonPropertyName("postalCode")]
    public virtual string PostalCode { get; set; }
}
