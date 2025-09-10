using CartEntities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace Store.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CheckoutService> _logger;
    private const string OrderSessionKey = "orders";

    public CheckoutService(ProtectedSessionStorage sessionStorage, ILogger<CheckoutService> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<Order> ProcessOrderAsync(Customer customer, Cart cart)
    {
        try
        {
            var order = new Order
            {
                Id = Random.Shared.Next(1000, 9999),
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Customer = customer,
                Items = new List<CartItem>(cart.Items),
                Subtotal = cart.Subtotal,
                Tax = cart.Tax,
                Total = cart.Total,
                Status = "Confirmed"
            };

            await SaveOrderAsync(order);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            throw;
        }
    }

    public async Task<Order?> GetOrderAsync(string orderNumber)
    {
        try
        {
            var orders = await GetOrdersAsync();
            return orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderNumber}", orderNumber);
            return null;
        }
    }

    private async Task SaveOrderAsync(Order order)
    {
        try
        {
            var orders = await GetOrdersAsync();
            orders.Add(order);
            
            var ordersJson = JsonSerializer.Serialize(orders);
            await _sessionStorage.SetAsync(OrderSessionKey, ordersJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order");
            throw;
        }
    }

    private async Task<List<Order>> GetOrdersAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(OrderSessionKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                var orders = JsonSerializer.Deserialize<List<Order>>(result.Value);
                return orders ?? new List<Order>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders from session storage");
        }
        
        return new List<Order>();
    }

    private static string GenerateOrderNumber()
    {
        return $"ESL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }
}