using CartEntities;
using DataEntities;
using SearchEntities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace Store.Services;

public class CartService : ICartService
{
    private readonly IProductService _productService;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CartService> _logger;
    private const string CartSessionKey = "cart";

    public CartService(IProductService productService, ProtectedSessionStorage sessionStorage, ILogger<CartService> logger)
    {
        _productService = productService;
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<Cart> GetCartAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(CartSessionKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                var cart = JsonSerializer.Deserialize<Cart>(result.Value);
                return cart ?? new Cart();
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
        {
            // During server-side rendering, JavaScript interop is not available
            // Return an empty cart - this will be populated when the component renders on the client
            _logger.LogDebug("JavaScript interop not available during server-side rendering, returning empty cart");
            return new Cart();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart from session storage");
        }
        
        return new Cart();
    }

    public async Task AddToCartAsync(int productId)
    {
        try
        {
            var products = await _productService.GetProducts();
            var product = products.FirstOrDefault(p => p.Id == productId);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", productId);
                return;
            }

            var cart = await GetCartAsync();
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                };
                cart.Items.Add(cartItem);
            }

            await SaveCartAsync(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
        }
    }

    public async Task UpdateQuantityAsync(int productId, int quantity)
    {
        try
        {
            var cart = await GetCartAsync();
            var item = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await SaveCartAsync(cart);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quantity for product {ProductId}", productId);
        }
    }

    public async Task RemoveFromCartAsync(int productId)
    {
        try
        {
            var cart = await GetCartAsync();
            var item = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                await SaveCartAsync(cart);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from cart", productId);
        }
    }

    public async Task ClearCartAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(CartSessionKey);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
        {
            // During server-side rendering, JavaScript interop is not available
            _logger.LogDebug("JavaScript interop not available during server-side rendering, cart not cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
        }
    }

    public async Task<int> GetCartItemCountAsync()
    {
        try
        {
            var cart = await GetCartAsync();
            return cart.ItemCount;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
        {
            // During server-side rendering, return 0
            _logger.LogDebug("JavaScript interop not available during server-side rendering, returning 0 for cart count");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart item count");
            return 0;
        }
    }

    private async Task SaveCartAsync(Cart cart)
    {
        try
        {
            var cartJson = JsonSerializer.Serialize(cart);
            await _sessionStorage.SetAsync(CartSessionKey, cartJson);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
        {
            // During server-side rendering, JavaScript interop is not available
            // This is expected and will be handled when the component renders on the client
            _logger.LogDebug("JavaScript interop not available during server-side rendering, cart not saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart to session storage");
        }
    }
}

// Create interface for existing ProductService to use with dependency injection
public interface IProductService
{
    Task<List<Product>> GetProducts();
    Task<SearchResponse?> Search(string searchTerm, bool semanticSearch = false);
}