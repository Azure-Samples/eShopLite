using CartEntities;

namespace Services;

public interface ICheckoutService
{
    Task<Order> ProcessOrderAsync(Customer customer, Cart cart);
    Task<Order?> GetOrderAsync(string orderNumber);
}