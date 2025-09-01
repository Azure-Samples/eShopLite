namespace PaymentsService.DTOs;

public class CreatePaymentRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? StoreId { get; set; }
    public string? CartId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public List<PaymentItemDto> Items { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public object? Metadata { get; set; }
}

public class PaymentItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}