using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace CartEntities;

public class Customer
{
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    public Address BillingAddress { get; set; } = new();
    
    public Address ShippingAddress { get; set; } = new();
    
    public bool SameAsShipping { get; set; } = true;
}