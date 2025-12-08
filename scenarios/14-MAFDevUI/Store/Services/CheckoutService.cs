using CartEntities;
using AgentServices.Checkout;
using AgentServices.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace Store.Services;

public class CheckoutService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly AgentCheckoutOrchestrator _agentOrchestrator;
    private readonly CustomerService _customerService;
    private readonly ILogger<CheckoutService> _logger;
    private const string OrderSessionKey = "orders";

    public CheckoutService(
        ProtectedSessionStorage sessionStorage,
        AgentCheckoutOrchestrator agentOrchestrator,
        CustomerService customerService,
        ILogger<CheckoutService> logger)
    {
        _sessionStorage = sessionStorage;
        _agentOrchestrator = agentOrchestrator;
        _customerService = customerService;
        _logger = logger;
    }

    // DEMO: Apply agentic checkout with discount computation
    public async Task<Cart> ApplyAgentCheckoutAsync(Cart cart)
    {
        try
        {
            _logger.LogInformation("DEMO: Starting agentic checkout for cart with {ItemCount} items", cart.ItemCount);
            
            var currentCustomer = _customerService.GetCurrentCustomer();
            _logger.LogInformation("DEMO: Current customer tier: {Tier}", currentCustomer.MembershipTier);

            var request = new AgentCheckoutRequest
            {
                Cart = cart,
                MembershipTier = currentCustomer.MembershipTier
            };

            var result = await _agentOrchestrator.ProcessCheckoutAsync(request);

            // Update cart with agent results
            cart.DiscountAmount = result.DiscountAmount;
            cart.DiscountReason = result.DiscountReason;
            cart.AgentSteps = result.AgentSteps;
            cart.WorkFlowMermaid = result.WorkFlowMermaid;

            _logger.LogInformation("DEMO: Agentic checkout completed - Discount: {Discount:C}, Reason: {Reason}",
                result.DiscountAmount, result.DiscountReason);

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DEMO: Agentic checkout failed, returning cart without discount");
            cart.DiscountAmount = 0;
            cart.DiscountReason = "Checkout running in standard mode";
            cart.AgentSteps = new List<AgentStep>
            {
                new AgentStep
                {
                    Name = "Orchestrator",
                    Status = "Warning",
                    Message = "Agent checkout unavailable, using standard mode",
                    Timestamp = DateTime.UtcNow
                }
            };
            return cart;
        }
    }

    public async Task<Order> ProcessOrderAsync(CartEntities.Customer customer, Cart cart)
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
                Status = "Confirmed",
                // DEMO: Include discount information from agentic checkout
                DiscountAmount = cart.DiscountAmount,
                DiscountReason = cart.DiscountReason,
                TotalAfterDiscount = cart.TotalAfterDiscount,
                AgentSteps = cart.AgentSteps
            };

            await SaveOrderAsync(order);
            _logger.LogInformation("DEMO: Order {OrderNumber} created with discount: {Discount:C}", 
                order.OrderNumber, order.DiscountAmount);
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
