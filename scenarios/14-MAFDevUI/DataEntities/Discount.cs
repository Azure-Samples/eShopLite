using System.Text.Json.Serialization;

namespace DataEntities;

public class Discount
{
    public Discount()
    {
        Id = 0;
        Name = "not defined";
        Description = "not defined";
        DiscountPercentage = 0;
        MembershipTier = MembershipTier.Normal;
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("name")]
    public virtual string Name { get; set; }

    [JsonPropertyName("description")]
    public virtual string Description { get; set; }

    [JsonPropertyName("discountPercentage")]
    public virtual decimal DiscountPercentage { get; set; }

    [JsonPropertyName("membershipTier")]
    public virtual MembershipTier MembershipTier { get; set; }
}
