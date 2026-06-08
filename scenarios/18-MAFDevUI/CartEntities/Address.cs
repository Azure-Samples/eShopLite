using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace CartEntities;

public class Address
{
    [Required]
    public string Street { get; set; } = string.Empty;
    
    [Required]
    public string City { get; set; } = string.Empty;
    
    [Required]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    public string Country { get; set; } = string.Empty;
}