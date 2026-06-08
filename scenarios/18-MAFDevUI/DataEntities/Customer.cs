using System.Text.Json.Serialization;

namespace DataEntities;

public enum MembershipTier
{
    Normal = 0,
    Silver = 1,
    Gold = 2
}

public class Customer
{
    public Customer()
    {
        Id = 0;
        FirstName = "not defined";
        LastName = "not defined";
        Email = "not defined";
        Phone = "not defined";
        MembershipTier = MembershipTier.Normal;
    }

    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("firstName")]
    public virtual string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public virtual string LastName { get; set; }

    [JsonPropertyName("email")]
    public virtual string Email { get; set; }

    [JsonPropertyName("phone")]
    public virtual string Phone { get; set; }

    [JsonPropertyName("membershipTier")]
    public virtual MembershipTier MembershipTier { get; set; }

    [JsonIgnore]
    public virtual string FullName => $"{FirstName} {LastName}";
}
