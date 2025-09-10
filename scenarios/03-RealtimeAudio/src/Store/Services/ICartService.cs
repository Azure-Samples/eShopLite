using CartEntities;

namespace Store.Services;

public interface ICartService
{
    Task<Cart> GetCartAsync();
    Task AddToCartAsync(int productId);
    Task UpdateQuantityAsync(int productId, int quantity);
    Task RemoveFromCartAsync(int productId);
    Task ClearCartAsync();
    Task<int> GetCartItemCountAsync();
}