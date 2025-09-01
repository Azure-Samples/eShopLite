namespace PaymentsService.Models;

public class PaymentRecord
{
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string? StoreId { get; set; }
    public string? CartId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ItemsJson { get; set; } = string.Empty;
    public string? ProductEnrichmentJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}