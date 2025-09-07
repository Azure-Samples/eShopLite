using System.Text.Json;
using CartEntities;

namespace Store.Services;

// DTOs that match the PaymentsService DTOs
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

public class CreatePaymentResponse
{
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

public interface IPaymentsClient
{
    Task<CreatePaymentResponse?> CreatePaymentAsync(CreatePaymentRequest request);
}

public class PaymentsClient : IPaymentsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentsClient> _logger;

    public PaymentsClient(HttpClient httpClient, ILogger<PaymentsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CreatePaymentResponse?> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payments", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var paymentResponse = JsonSerializer.Deserialize<CreatePaymentResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Payment created successfully with ID: {PaymentId}", paymentResponse?.PaymentId);
                return paymentResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create payment. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating payment for user {UserId}", request.UserId);
            return null;
        }
    }

    // Helper method to convert Cart to CreatePaymentRequest
    public static CreatePaymentRequest FromCart(Cart cart, Customer customer, string paymentMethod)
    {
        return new CreatePaymentRequest
        {
            UserId = customer.Email ?? "anonymous", // Using email as userId
            StoreId = "eShopLite",
            CartId = "session-cart", // Since cart is session-based
            Currency = "USD",
            Amount = cart.Total,
            PaymentMethod = paymentMethod,
            Items = cart.Items.Select(item => new PaymentItemDto
            {
                ProductId = item.ProductId.ToString(),
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList()
        };
    }
}