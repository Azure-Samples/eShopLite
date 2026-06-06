# Product Requirements Document (PRD)
## Shopping Cart Feature for eShopLite Application

### Document Information
- **Product**: eShopLite - Semantic Search Application
- **Feature**: Shopping Cart Functionality
- **Version**: 1.0
- **Date**: July 5, 2025
- **Status**: Draft

---

## 1. Executive Summary

### 1.1 Overview
This PRD outlines the requirements for adding a shopping cart feature to the existing eShopLite application. The shopping cart will allow users to add products, manage quantities, view totals, and complete purchases without requiring user authentication or inventory management.

### 1.2 Success Metrics
- Users can successfully add products to cart
- Users can modify cart contents (add/remove/update quantities)
- Cart persists during browser session
- Purchase completion rate > 90%
- Cart abandonment tracking available

---

## 2. Background & Context

### 2.1 Current Application State
The eShopLite application currently provides:
- Product catalog browsing
- Keyword and semantic search functionality
- Product detail viewing
- .NET Aspire hosting with Azure OpenAI integration
- Blazor Server-side rendering
- SQLite database for product storage

### 2.2 Business Justification
Adding a shopping cart feature will:
- Complete the e-commerce experience
- Enable transaction completion
- Provide foundation for future payment integration
- Demonstrate end-to-end commerce capabilities

---

## 3. Product Requirements

### 3.1 Functional Requirements

#### 3.1.1 Cart Management
**FR-01: Add to Cart**
- Users can add products to cart from the product listing page
- Users can specify quantity when adding products
- System validates positive integer quantities
- Visual feedback confirms successful addition

**FR-02: View Cart**
- Users can access cart through navigation menu/header
- Cart displays: product name, image, price, quantity, subtotal
- Cart shows total item count and grand total
- Empty cart state displays appropriate message

**FR-03: Modify Cart Contents**
- Users can update product quantities in cart
- Users can remove individual products from cart
- Users can clear entire cart
- Real-time total calculations update automatically

**FR-04: Cart Persistence**
- Cart contents persist during browser session
- Cart contents lost when browser session ends (no cross-session persistence)
- No user account required

#### 3.1.2 Checkout Process
**FR-05: Checkout Initiation**
- Users can proceed to checkout from cart page
- Checkout requires minimum cart value (configurable, default: $0.01)
- System validates cart not empty before checkout

**FR-06: Customer Information Collection**
- Collect customer details at checkout: Name, Email, Phone, Shipping Address, Billing Address
- All fields mandatory except separate billing address
- Email format validation required
- Phone number format validation required

**FR-07: Order Summary**
- Display final order summary before confirmation
- Show all products, quantities, prices, and totals
- Include customer information for verification
- Provide edit functionality to return to cart or customer info

**FR-08: Order Completion**
- Generate unique order confirmation number
- Display order confirmation page with details
- Clear cart after successful order placement
- No payment processing (future enhancement)

#### 3.1.3 Product Integration
**FR-09: Product Display Enhancement**
- Add "Add to Cart" buttons to product listing page
- Include quantity selector on product pages
- Display current cart count in application header
- Show product availability (always available - infinite stock)

### 3.2 Non-Functional Requirements

#### 3.2.1 Performance
**NFR-01: Response Time**
- Cart operations complete within 2 seconds
- Page loads complete within 3 seconds
- Real-time cart updates within 1 second

**NFR-02: Scalability**
- Support concurrent cart operations
- Handle multiple users simultaneously
- Cart data stored in memory/session (not persisted)

#### 3.2.1 Usability
**NFR-03: User Experience**
- Intuitive cart icon with item count
- Clear visual indicators for cart actions
- Responsive design for mobile/desktop
- Accessibility compliance (WCAG 2.1 AA)

**NFR-04: Browser Compatibility**
- Support modern browsers (Chrome, Firefox, Safari, Edge)
- Progressive enhancement for older browsers

#### 3.2.3 Security
**NFR-05: Data Protection**
- No sensitive data persistence
- Customer data validation and sanitization
- Protection against XSS and injection attacks

---

## 4. Technical Architecture

### 4.1 Data Models

#### 4.1.1 New Entities Required

**CartItem Entity**
```csharp
public class CartItem
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
```

**Cart Entity**
```csharp
public class Cart
{
    public string SessionId { get; set; }
    public List<CartItem> Items { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public int TotalItems => Items.Sum(i => i.Quantity);
}
```

**Customer Entity**
```csharp
public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public Address ShippingAddress { get; set; }
    public Address BillingAddress { get; set; }
}
```

**Address Entity**
```csharp
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
}
```

**Order Entity**
```csharp
public class Order
{
    public string OrderId { get; set; }
    public Customer Customer { get; set; }
    public List<CartItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
}
```

### 4.2 New Components Required

#### 4.2.1 Blazor Components
- `CartComponent.razor` - Cart display and management
- `CartSummary.razor` - Mini cart for header
- `CheckoutForm.razor` - Customer information collection
- `OrderSummary.razor` - Final order review
- `OrderConfirmation.razor` - Order completion page

#### 4.2.2 Services
- `CartService.cs` - Cart management operations
- `CheckoutService.cs` - Order processing logic
- `SessionService.cs` - Session management

#### 4.2.3 API Endpoints
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item quantity
- `DELETE /api/cart/remove/{productId}` - Remove item from cart
- `DELETE /api/cart/clear` - Clear entire cart
- `GET /api/cart` - Get current cart contents
- `POST /api/orders` - Create new order

### 4.3 Database Changes
- No database schema changes required
- Cart data stored in memory/session
- Orders can be logged to existing SQLite database (optional)

---

## 5. User Experience Design

### 5.1 User Journey
1. **Browse Products** → View product catalog
2. **Add to Cart** → Select products and quantities
3. **Review Cart** → Modify contents as needed
4. **Checkout** → Enter customer information
5. **Confirm Order** → Review and place order
6. **Order Confirmation** → Receive confirmation details

### 5.2 Key UI Components

#### 5.2.1 Cart Icon (Header)
- Shopping cart icon with item count badge
- Click to view cart summary dropdown
- Always visible in application header

#### 5.2.2 Add to Cart Button
- Prominent button on product listings
- Quantity selector (default: 1)
- Loading state during addition
- Success feedback animation

#### 5.2.3 Cart Page
- Full cart view with product details
- Quantity adjustment controls (+/- buttons)
- Remove item functionality
- Subtotals and grand total
- "Continue Shopping" and "Checkout" buttons

#### 5.2.4 Checkout Flow
- Multi-step process with progress indicator
- Form validation with inline error messages
- Order summary sidebar
- Clear call-to-action buttons

---

## 6. Implementation Plan

### 6.1 Development Phases

#### Phase 1: Core Cart Functionality (Week 1-2)
- Implement CartItem and Cart entities
- Create CartService for basic operations
- Add "Add to Cart" buttons to product pages
- Implement cart storage in session

#### Phase 2: Cart Management UI (Week 3)
- Build cart page with full functionality
- Implement cart header component
- Add quantity modification features
- Create cart clearing functionality

#### Phase 3: Checkout Process (Week 4-5)
- Implement customer information collection
- Create order summary page
- Build order confirmation flow
- Add form validation

#### Phase 4: Integration & Testing (Week 6)
- Integration testing across components
- User acceptance testing
- Performance optimization
- Bug fixes and refinements

### 6.2 Technical Dependencies
- Existing Product API and entities
- Blazor Server framework
- Session state management
- Form validation libraries

---

## 7. Testing Strategy

### 7.1 Unit Testing
- Cart service operations
- Order creation logic
- Validation functions
- Entity model tests

### 7.2 Integration Testing
- API endpoint functionality
- Database interactions (if order persistence added)
- Session management
- Cart-to-order flow

### 7.3 User Acceptance Testing
- Complete user journey testing
- Cross-browser compatibility
- Mobile responsiveness
- Accessibility validation

### 7.4 Performance Testing
- Cart operation response times
- Concurrent user scenarios
- Memory usage with large carts
- Session storage limitations

---

## 8. Risks & Mitigation

### 8.1 Technical Risks
**Risk**: Session storage limitations
**Mitigation**: Implement cart size limits and user messaging

**Risk**: Browser compatibility issues
**Mitigation**: Progressive enhancement and fallback options

**Risk**: Performance degradation with large carts
**Mitigation**: Implement cart size limits and optimization

### 8.2 User Experience Risks
**Risk**: Cart abandonment due to complex checkout
**Mitigation**: Streamlined, single-page checkout process

**Risk**: Lost cart contents on browser crash
**Mitigation**: Clear user messaging about session-based storage

---

## 9. Future Enhancements

### 9.1 Immediate Next Steps (Post-Launch)
- Payment integration (Stripe, PayPal)
- Order persistence and history
- Email confirmations
- Inventory management integration

### 9.2 Advanced Features
- User accounts and authentication
- Saved carts and wish lists
- Product recommendations
- Advanced order tracking
- Customer support integration

---

## 10. Success Criteria

### 10.1 Launch Criteria
- [ ] All functional requirements implemented
- [ ] 95% test coverage achieved
- [ ] Performance benchmarks met
- [ ] Accessibility compliance verified
- [ ] Cross-browser testing completed

### 10.2 Post-Launch Metrics
- Cart abandonment rate < 70%
- Order completion rate > 90%
- Average cart value tracking
- User journey completion analytics
- Performance monitoring alerts

---

## 11. Appendices

### 11.1 Wireframes
*(To be added during design phase)*

### 11.2 API Documentation
*(Detailed API specifications to be created during development)*

### 11.3 Database Schema
*(If order persistence is implemented)*

---

**Document Approval:**
- [ ] Product Manager
- [ ] Technical Lead  
- [ ] UX Designer
- [ ] Development Team

**Next Steps:**
1. Review and approve PRD
2. Create detailed technical specifications
3. Begin Phase 1 development
4. Set up project tracking and milestones

---

## 12. Detailed Implementation Guide

### 12.1 Phase 1: Core Cart Functionality (Week 1-2)

#### Step 1.1: Create New Projects and Entities

**Create CartEntities Project:**
```bash
cd src
dotnet new classlib -n CartEntities
dotnet sln add CartEntities/CartEntities.csproj
```

**Add Required NuGet Packages:**
```xml
<!-- CartEntities/CartEntities.csproj -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />
<PackageReference Include="System.ComponentModel.DataAnnotations" Version="5.0.0" />
```

**Create Entity Classes:**

1. **CartItem.cs**
```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DataEntities;

namespace CartEntities;

public class CartItem
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }
    
    [JsonPropertyName("product")]
    public Product? Product { get; set; }
    
    [JsonPropertyName("quantity")]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
    
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
    
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice => Quantity * UnitPrice;
    
    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
```

2. **Cart.cs**
```csharp
using System.Text.Json.Serialization;

namespace CartEntities;

public class Cart
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("items")]
    public List<CartItem> Items { get; set; } = new();
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    
    [JsonPropertyName("totalItems")]
    public int TotalItems => Items.Sum(i => i.Quantity);
    
    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }
    
    public bool UpdateQuantity(int productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null && quantity > 0)
        {
            item.Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }
    
    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
```

3. **Customer.cs**
```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CartEntities;

public class Customer
{
    [JsonPropertyName("firstName")]
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("lastName")]
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("phone")]
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string Phone { get; set; } = string.Empty;
    
    [JsonPropertyName("shippingAddress")]
    [Required(ErrorMessage = "Shipping address is required")]
    public Address ShippingAddress { get; set; } = new();
    
    [JsonPropertyName("billingAddress")]
    public Address? BillingAddress { get; set; }
    
    [JsonPropertyName("fullName")]
    public string FullName => $"{FirstName} {LastName}";
}
```

4. **Address.cs**
```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CartEntities;

public class Address
{
    [JsonPropertyName("street")]
    [Required(ErrorMessage = "Street address is required")]
    [StringLength(100, ErrorMessage = "Street address cannot exceed 100 characters")]
    public string Street { get; set; } = string.Empty;
    
    [JsonPropertyName("city")]
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string City { get; set; } = string.Empty;
    
    [JsonPropertyName("state")]
    [Required(ErrorMessage = "State is required")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    public string State { get; set; } = string.Empty;
    
    [JsonPropertyName("zipCode")]
    [Required(ErrorMessage = "ZIP code is required")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
    public string ZipCode { get; set; } = string.Empty;
    
    [JsonPropertyName("country")]
    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
    public string Country { get; set; } = "United States";
}
```

5. **Order.cs**
```csharp
using System.Text.Json.Serialization;

namespace CartEntities;

public class Order
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;
    
    [JsonPropertyName("customer")]
    public Customer Customer { get; set; } = new();
    
    [JsonPropertyName("items")]
    public List<CartItem> Items { get; set; } = new();
    
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
    
    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public static string GenerateOrderId()
    {
        return $"ESL{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

#### Step 1.2: Create Cart Service

**Add CartService to Store project:**

1. **ICartService.cs**
```csharp
using CartEntities;

namespace Store.Services;

public interface ICartService
{
    Task<Cart> GetCartAsync();
    Task AddToCartAsync(int productId, int quantity = 1);
    Task<bool> UpdateQuantityAsync(int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int productId);
    Task ClearCartAsync();
    Task<int> GetCartItemCountAsync();
    event EventHandler<Cart>? CartChanged;
}
```

2. **CartService.cs**
```csharp
using CartEntities;
using DataEntities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace Store.Services;

public class CartService : ICartService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ProductService _productService;
    private readonly ILogger<CartService> _logger;
    private const string CART_KEY = "eShopLite_Cart";
    
    public event EventHandler<Cart>? CartChanged;
    
    public CartService(
        ProtectedSessionStorage sessionStorage,
        ProductService productService,
        ILogger<CartService> logger)
    {
        _sessionStorage = sessionStorage;
        _productService = productService;
        _logger = logger;
    }
    
    public async Task<Cart> GetCartAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(CART_KEY);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                var cart = JsonSerializer.Deserialize<Cart>(result.Value);
                return cart ?? new Cart { SessionId = Guid.NewGuid().ToString() };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart from session storage");
        }
        
        return new Cart { SessionId = Guid.NewGuid().ToString() };
    }
    
    public async Task AddToCartAsync(int productId, int quantity = 1)
    {
        try
        {
            var cart = await GetCartAsync();
            var products = await _productService.GetProducts();
            var product = products.FirstOrDefault(p => p.Id == productId);
            
            if (product != null)
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                
                cart.AddItem(cartItem);
                await SaveCartAsync(cart);
                CartChanged?.Invoke(this, cart);
                
                _logger.LogInformation($"Added {quantity} of product {productId} to cart");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding product {productId} to cart");
            throw;
        }
    }
    
    public async Task<bool> UpdateQuantityAsync(int productId, int quantity)
    {
        try
        {
            var cart = await GetCartAsync();
            var updated = cart.UpdateQuantity(productId, quantity);
            
            if (updated)
            {
                await SaveCartAsync(cart);
                CartChanged?.Invoke(this, cart);
                _logger.LogInformation($"Updated product {productId} quantity to {quantity}");
            }
            
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating quantity for product {productId}");
            return false;
        }
    }
    
    public async Task<bool> RemoveFromCartAsync(int productId)
    {
        try
        {
            var cart = await GetCartAsync();
            var removed = cart.RemoveItem(productId);
            
            if (removed)
            {
                await SaveCartAsync(cart);
                CartChanged?.Invoke(this, cart);
                _logger.LogInformation($"Removed product {productId} from cart");
            }
            
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing product {productId} from cart");
            return false;
        }
    }
    
    public async Task ClearCartAsync()
    {
        try
        {
            var cart = await GetCartAsync();
            cart.Clear();
            await SaveCartAsync(cart);
            CartChanged?.Invoke(this, cart);
            _logger.LogInformation("Cart cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            throw;
        }
    }
    
    public async Task<int> GetCartItemCountAsync()
    {
        var cart = await GetCartAsync();
        return cart.TotalItems;
    }
    
    private async Task SaveCartAsync(Cart cart)
    {
        try
        {
            var cartJson = JsonSerializer.Serialize(cart);
            await _sessionStorage.SetAsync(CART_KEY, cartJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart to session storage");
            throw;
        }
    }
}
```

#### Step 1.3: Update Store Project Configuration

**Update Store/Program.cs:**
```csharp
using Store.Components;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// Add existing services
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

// Add cart services
builder.Services.AddScoped<ICartService, CartService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### 12.2 Phase 2: Cart Management UI (Week 3)

#### Step 2.1: Create Cart Components

1. **Components/Cart/CartSummary.razor**
```aspnetcorerazor
@using CartEntities
@using Store.Services
@inject ICartService CartService
@implements IDisposable

<div class="cart-summary">
    <button class="btn btn-outline-primary position-relative" @onclick="ToggleCartDropdown">
        <i class="fas fa-shopping-cart"></i>
        @if (cartItemCount > 0)
        {
            <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                @cartItemCount
                <span class="visually-hidden">items in cart</span>
            </span>
        }
    </button>
    
    @if (showDropdown && cart != null)
    {
        <div class="cart-dropdown">
            <div class="cart-dropdown-header">
                <h6>Shopping Cart (@cart.TotalItems items)</h6>
            </div>
            <div class="cart-dropdown-body">
                @if (cart.Items.Any())
                {
                    @foreach (var item in cart.Items.Take(3))
                    {
                        <div class="cart-item-summary">
                            <img src="https://raw.githubusercontent.com/MicrosoftDocs/mslearn-dotnet-cloudnative/main/dotnet-docker/Products/wwwroot/images/@item.Product?.ImageUrl" 
                                 alt="@item.Product?.Name" class="cart-item-image" />
                            <div class="cart-item-details">
                                <div class="cart-item-name">@item.Product?.Name</div>
                                <div class="cart-item-price">@item.Quantity x @item.UnitPrice.ToString("C")</div>
                            </div>
                        </div>
                    }
                    
                    @if (cart.Items.Count > 3)
                    {
                        <div class="text-muted">... and @(cart.Items.Count - 3) more items</div>
                    }
                    
                    <hr />
                    <div class="cart-total">
                        <strong>Total: @cart.TotalAmount.ToString("C")</strong>
                    </div>
                }
                else
                {
                    <div class="text-muted">Your cart is empty</div>
                }
            </div>
            <div class="cart-dropdown-footer">
                <a href="/cart" class="btn btn-primary btn-sm w-100">View Cart</a>
            </div>
        </div>
    }
</div>

@code {
    private Cart? cart;
    private int cartItemCount = 0;
    private bool showDropdown = false;
    
    protected override async Task OnInitializedAsync()
    {
        cart = await CartService.GetCartAsync();
        cartItemCount = cart.TotalItems;
        CartService.CartChanged += OnCartChanged;
    }
    
    private async void OnCartChanged(object? sender, Cart updatedCart)
    {
        cart = updatedCart;
        cartItemCount = cart.TotalItems;
        await InvokeAsync(StateHasChanged);
    }
    
    private void ToggleCartDropdown()
    {
        showDropdown = !showDropdown;
    }
    
    public void Dispose()
    {
        CartService.CartChanged -= OnCartChanged;
    }
}

<style>
    .cart-summary {
        position: relative;
    }
    
    .cart-dropdown {
        position: absolute;
        top: 100%;
        right: 0;
        background: white;
        border: 1px solid #ddd;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        width: 300px;
        z-index: 1000;
        margin-top: 5px;
    }
    
    .cart-dropdown-header,
    .cart-dropdown-footer {
        padding: 12px;
        border-bottom: 1px solid #eee;
    }
    
    .cart-dropdown-footer {
        border-bottom: none;
        border-top: 1px solid #eee;
    }
    
    .cart-dropdown-body {
        padding: 12px;
        max-height: 300px;
        overflow-y: auto;
    }
    
    .cart-item-summary {
        display: flex;
        align-items: center;
        margin-bottom: 10px;
    }
    
    .cart-item-image {
        width: 40px;
        height: 40px;
        object-fit: cover;
        border-radius: 4px;
        margin-right: 10px;
    }
    
    .cart-item-details {
        flex: 1;
    }
    
    .cart-item-name {
        font-size: 14px;
        font-weight: 500;
    }
    
    .cart-item-price {
        font-size: 12px;
        color: #666;
    }
    
    .cart-total {
        text-align: center;
        font-size: 16px;
    }
</style>
```

2. **Components/Pages/Cart.razor**
```aspnetcorerazor
@page "/cart"
@using CartEntities
@using Store.Services
@inject ICartService CartService
@inject NavigationManager Navigation
@attribute [StreamRendering(true)]

<PageTitle>Shopping Cart</PageTitle>

<div class="container mt-4">
    <div class="row">
        <div class="col-lg-8">
            <h2>Shopping Cart</h2>
            
            @if (cart == null)
            {
                <div class="text-center">
                    <div class="spinner-border" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            }
            else if (!cart.Items.Any())
            {
                <div class="empty-cart text-center py-5">
                    <i class="fas fa-shopping-cart fa-3x text-muted mb-3"></i>
                    <h4>Your cart is empty</h4>
                    <p class="text-muted">Add some products to get started</p>
                    <a href="/products" class="btn btn-primary">Continue Shopping</a>
                </div>
            }
            else
            {
                <div class="cart-items">
                    @foreach (var item in cart.Items)
                    {
                        <div class="cart-item card mb-3">
                            <div class="card-body">
                                <div class="row align-items-center">
                                    <div class="col-md-2">
                                        <img src="https://raw.githubusercontent.com/MicrosoftDocs/mslearn-dotnet-cloudnative/main/dotnet-docker/Products/wwwroot/images/@item.Product?.ImageUrl" 
                                             alt="@item.Product?.Name" class="img-fluid rounded" />
                                    </div>
                                    <div class="col-md-4">
                                        <h5>@item.Product?.Name</h5>
                                        <p class="text-muted">@item.Product?.Description</p>
                                    </div>
                                    <div class="col-md-2">
                                        <div class="price">@item.UnitPrice.ToString("C")</div>
                                    </div>
                                    <div class="col-md-2">
                                        <div class="quantity-controls">
                                            <div class="input-group">
                                                <button class="btn btn-outline-secondary" type="button" 
                                                        @onclick="() => UpdateQuantity(item.ProductId, item.Quantity - 1)">
                                                    <i class="fas fa-minus"></i>
                                                </button>
                                                <input type="number" class="form-control text-center" 
                                                       value="@item.Quantity" min="1" max="100"
                                                       @onchange="(e) => UpdateQuantity(item.ProductId, int.Parse(e.Value?.ToString() ?? \"1\"))" />
                                                <button class="btn btn-outline-secondary" type="button"
                                                        @onclick="() => UpdateQuantity(item.ProductId, item.Quantity + 1)">
                                                    <i class="fas fa-plus"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-1">
                                        <div class="subtotal">@item.TotalPrice.ToString("C")</div>
                                    </div>
                                    <div class="col-md-1">
                                        <button class="btn btn-outline-danger btn-sm" 
                                                @onclick="() => RemoveItem(item.ProductId)">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
                
                <div class="cart-actions mt-4">
                    <div class="row">
                        <div class="col-md-6">
                            <a href="/products" class="btn btn-outline-primary">
                                <i class="fas fa-arrow-left"></i> Continue Shopping
                            </a>
                        </div>
                        <div class="col-md-6 text-end">
                            <button class="btn btn-outline-secondary me-2" @onclick="ClearCart">
                                Clear Cart
                            </button>
                        </div>
                    </div>
                </div>
            }
        </div>
        
        @if (cart != null && cart.Items.Any())
        {
            <div class="col-lg-4">
                <div class="card">
                    <div class="card-header">
                        <h5>Order Summary</h5>
                    </div>
                    <div class="card-body">
                        <div class="summary-row">
                            <span>Items (@cart.TotalItems):</span>
                            <span>@cart.TotalAmount.ToString("C")</span>
                        </div>
                        <div class="summary-row">
                            <span>Shipping:</span>
                            <span class="text-success">FREE</span>
                        </div>
                        <hr />
                        <div class="summary-row total">
                            <strong>
                                <span>Total:</span>
                                <span>@cart.TotalAmount.ToString("C")</span>
                            </strong>
                        </div>
                        <button class="btn btn-success w-100 mt-3" @onclick="ProceedToCheckout">
                            Proceed to Checkout
                        </button>
                    </div>
                </div>
            </div>
        }
    </div>
</div>

@code {
    private Cart? cart;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadCart();
    }
    
    private async Task LoadCart()
    {
        cart = await CartService.GetCartAsync();
    }
    
    private async Task UpdateQuantity(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            await RemoveItem(productId);
            return;
        }
        
        await CartService.UpdateQuantityAsync(productId, quantity);
        await LoadCart();
    }
    
    private async Task RemoveItem(int productId)
    {
        await CartService.RemoveFromCartAsync(productId);
        await LoadCart();
    }
    
    private async Task ClearCart()
    {
        await CartService.ClearCartAsync();
        await LoadCart();
    }
    
    private void ProceedToCheckout()
    {
        Navigation.NavigateTo("/checkout");
    }
}

<style>
    .empty-cart {
        background: #f8f9fa;
        border-radius: 8px;
    }
    
    .cart-item {
        transition: all 0.2s ease;
    }
    
    .cart-item:hover {
        box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }
    
    .quantity-controls .input-group {
        width: 120px;
    }
    
    .quantity-controls input {
        max-width: 60px;
    }
    
    .price, .subtotal {
        font-weight: 600;
        font-size: 1.1em;
    }
    
    .summary-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 10px;
    }
    
    .summary-row.total {
        font-size: 1.2em;
        margin-bottom: 0;
    }
</style>
```

#### Step 2.2: Update Product Listing Page

**Update Store/Components/Pages/Products.razor:**
```aspnetcorerazor
@page "/products"
@using DataEntities
@using Store.Services
@inject ProductService ProductService
@inject ICartService CartService
@inject IConfiguration Configuration
@attribute [StreamRendering(true)]

<PageTitle>Products</PageTitle>

<h1>Products</h1>

<p>Here are some of our amazing outdoor products that you can purchase.</p>

@if (products == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <div class="row">
        @foreach (var product in products)
        {
            <div class="col-lg-4 col-md-6 mb-4">
                <div class="card h-100">
                    <img src="https://raw.githubusercontent.com/MicrosoftDocs/mslearn-dotnet-cloudnative/main/dotnet-docker/Products/wwwroot/images/@product.ImageUrl" 
                         class="card-img-top" alt="@product.Name" style="height: 200px; object-fit: cover;" />
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title">@product.Name</h5>
                        <p class="card-text flex-grow-1">@product.Description</p>
                        <div class="product-price mb-3">
                            <h4 class="text-primary">@product.Price.ToString("C")</h4>
                        </div>
                        <div class="add-to-cart-section">
                            <div class="row">
                                <div class="col-4">
                                    <input type="number" class="form-control" @bind="quantities[product.Id]" 
                                           min="1" max="100" value="1" />
                                </div>
                                <div class="col-8">
                                    <button class="btn btn-primary w-100" 
                                            @onclick="() => AddToCart(product.Id)"
                                            disabled="@addingToCart">
                                        @if (addingToCart)
                                        {
                                            <span class="spinner-border spinner-border-sm me-2"></span>
                                        }
                                        Add to Cart
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        }
    </div>
}

@if (showSuccessMessage)
{
    <div class="toast-container position-fixed bottom-0 end-0 p-3">
        <div class="toast show">
            <div class="toast-header">
                <i class="fas fa-check-circle text-success me-2"></i>
                <strong class="me-auto">Success</strong>
                <button type="button" class="btn-close" @onclick="() => showSuccessMessage = false"></button>
            </div>
            <div class="toast-body">
                Product added to cart successfully!
            </div>
        </div>
    </div>
}

@code {
    private List<Product>? products;
    private Dictionary<int, int> quantities = new();
    private bool addingToCart = false;
    private bool showSuccessMessage = false;

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(500);
        products = await ProductService.GetProducts();
        
        // Initialize quantities dictionary
        if (products != null)
        {
            foreach (var product in products)
            {
                quantities[product.Id] = 1;
            }
        }
    }
    
    private async Task AddToCart(int productId)
    {
        addingToCart = true;
        try
        {
            var quantity = quantities.ContainsKey(productId) ? quantities[productId] : 1;
            await CartService.AddToCartAsync(productId, quantity);
            
            showSuccessMessage = true;
            
            // Hide success message after 3 seconds
            _ = Task.Delay(3000).ContinueWith(_ => 
            {
                showSuccessMessage = false;
                InvokeAsync(StateHasChanged);
            });
        }
        catch (Exception ex)
        {
            // Handle error (could show error toast)
            Console.WriteLine($"Error adding to cart: {ex.Message}");
        }
        finally
        {
            addingToCart = false;
        }
    }
}
```

### 12.3 Phase 3: Checkout Process (Week 4-5)

#### Step 3.1: Create Checkout Service

**Store/Services/CheckoutService.cs:**
```csharp
using CartEntities;

namespace Store.Services;

public interface ICheckoutService
{
    Task<Order> CreateOrderAsync(Cart cart, Customer customer);
    Task<bool> ValidateCartAsync(Cart cart);
    Task<bool> ValidateCustomerAsync(Customer customer);
}

public class CheckoutService : ICheckoutService
{
    private readonly ILogger<CheckoutService> _logger;
    
    public CheckoutService(ILogger<CheckoutService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Order> CreateOrderAsync(Cart cart, Customer customer)
    {
        if (!await ValidateCartAsync(cart))
            throw new InvalidOperationException("Invalid cart");
            
        if (!await ValidateCustomerAsync(customer))
            throw new InvalidOperationException("Invalid customer information");
        
        var order = new Order
        {
            OrderId = Order.GenerateOrderId(),
            Customer = customer,
            Items = cart.Items.ToList(),
            TotalAmount = cart.TotalAmount,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };
        
        _logger.LogInformation($"Order created: {order.OrderId} for {customer.Email}");
        
        return order;
    }
    
    public async Task<bool> ValidateCartAsync(Cart cart)
    {
        return await Task.FromResult(cart?.Items?.Any() == true && cart.TotalAmount > 0);
    }
    
    public async Task<bool> ValidateCustomerAsync(Customer customer)
    {
        if (customer == null) return false;
        
        var isValid = !string.IsNullOrWhiteSpace(customer.FirstName) &&
                     !string.IsNullOrWhiteSpace(customer.LastName) &&
                     !string.IsNullOrWhiteSpace(customer.Email) &&
                     !string.IsNullOrWhiteSpace(customer.Phone) &&
                     customer.ShippingAddress != null &&
                     !string.IsNullOrWhiteSpace(customer.ShippingAddress.Street) &&
                     !string.IsNullOrWhiteSpace(customer.ShippingAddress.City) &&
                     !string.IsNullOrWhiteSpace(customer.ShippingAddress.State) &&
                     !string.IsNullOrWhiteSpace(customer.ShippingAddress.ZipCode);
        
        return await Task.FromResult(isValid);
    }
}
```

This implementation guide provides the complete foundation for a fully functional shopping cart experience. Each phase builds upon the previous one, ensuring a systematic approach to development. The next phase would involve completing the checkout UI components and order confirmation pages.

#### Step 3.2: Create Checkout Components

**Components/Pages/Checkout.razor:**
```aspnetcorerazor
@page "/checkout"
@using CartEntities
@using Store.Services
@using System.ComponentModel.DataAnnotations
@inject ICartService CartService
@inject ICheckoutService CheckoutService
@inject NavigationManager Navigation
@attribute [StreamRendering(true)]

<PageTitle>Checkout</PageTitle>

<div class="container mt-4">
    @if (cart == null || !cart.Items.Any())
    {
        <div class="alert alert-warning">
            <h4>Your cart is empty</h4>
            <p>Please add some items to your cart before proceeding to checkout.</p>
            <a href="/products" class="btn btn-primary">Continue Shopping</a>
        </div>
    }
    else
    {
        <div class="row">
            <div class="col-lg-8">
                <h2>Checkout</h2>
                
                <!-- Progress Indicator -->
                <div class="checkout-progress mb-4">
                    <div class="progress-step @(currentStep >= 1 ? "active" : "")">
                        <div class="step-number">1</div>
                        <div class="step-label">Customer Info</div>
                    </div>
                    <div class="progress-step @(currentStep >= 2 ? "active" : "")">
                        <div class="step-number">2</div>
                        <div class="step-label">Review Order</div>
                    </div>
                    <div class="progress-step @(currentStep >= 3 ? "active" : "")">
                        <div class="step-number">3</div>
                        <div class="step-label">Confirmation</div>
                    </div>
                </div>

                @if (currentStep == 1)
                {
                    <!-- Customer Information Form -->
                    <div class="card">
                        <div class="card-header">
                            <h5>Customer Information</h5>
                        </div>
                        <div class="card-body">
                            <EditForm Model="customer" OnValidSubmit="ProceedToReview">
                                <DataAnnotationsValidator />
                                
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="form-label">First Name *</label>
                                        <InputText @bind-Value="customer.FirstName" class="form-control" />
                                        <ValidationMessage For="() => customer.FirstName" />
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Last Name *</label>
                                        <InputText @bind-Value="customer.LastName" class="form-control" />
                                        <ValidationMessage For="() => customer.LastName" />
                                    </div>
                                </div>
                                
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="form-label">Email *</label>
                                        <InputText @bind-Value="customer.Email" class="form-control" type="email" />
                                        <ValidationMessage For="() => customer.Email" />
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Phone *</label>
                                        <InputText @bind-Value="customer.Phone" class="form-control" />
                                        <ValidationMessage For="() => customer.Phone" />
                                    </div>
                                </div>
                                
                                <h6 class="mt-4 mb-3">Shipping Address</h6>
                                <div class="mb-3">
                                    <label class="form-label">Street Address *</label>
                                    <InputText @bind-Value="customer.ShippingAddress.Street" class="form-control" />
                                    <ValidationMessage For="() => customer.ShippingAddress.Street" />
                                </div>
                                
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label class="form-label">City *</label>
                                        <InputText @bind-Value="customer.ShippingAddress.City" class="form-control" />
                                        <ValidationMessage For="() => customer.ShippingAddress.City" />
                                    </div>
                                    <div class="col-md-3">
                                        <label class="form-label">State *</label>
                                        <InputText @bind-Value="customer.ShippingAddress.State" class="form-control" />
                                        <ValidationMessage For="() => customer.ShippingAddress.State" />
                                    </div>
                                    <div class="col-md-3">
                                        <label class="form-label">ZIP Code *</label>
                                        <InputText @bind-Value="customer.ShippingAddress.ZipCode" class="form-control" />
                                        <ValidationMessage For="() => customer.ShippingAddress.ZipCode" />
                                    </div>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="form-label">Country *</label>
                                    <InputText @bind-Value="customer.ShippingAddress.Country" class="form-control" />
                                    <ValidationMessage For="() => customer.ShippingAddress.Country" />
                                </div>
                                
                                <div class="d-flex justify-content-between">
                                    <a href="/cart" class="btn btn-outline-secondary">
                                        <i class="fas fa-arrow-left"></i> Back to Cart
                                    </a>
                                    <button type="submit" class="btn btn-primary">
                                        Continue to Review <i class="fas fa-arrow-right"></i>
                                    </button>
                                </div>
                            </EditForm>
                        </div>
                    </div>
                }
                else if (currentStep == 2)
                {
                    <!-- Order Review -->
                    <div class="card">
                        <div class="card-header">
                            <h5>Review Your Order</h5>
                        </div>
                        <div class="card-body">
                            <!-- Customer Information Review -->
                            <h6>Customer Information</h6>
                            <div class="customer-info-review mb-4">
                                <p><strong>@customer.FullName</strong></p>
                                <p>@customer.Email | @customer.Phone</p>
                                <p>
                                    @customer.ShippingAddress.Street<br/>
                                    @customer.ShippingAddress.City, @customer.ShippingAddress.State @customer.ShippingAddress.ZipCode<br/>
                                    @customer.ShippingAddress.Country
                                </p>
                                <button class="btn btn-sm btn-outline-primary" @onclick="() => currentStep = 1">
                                    Edit Information
                                </button>
                            </div>
                            
                            <!-- Order Items Review -->
                            <h6>Order Items</h6>
                            <div class="order-items-review">
                                @foreach (var item in cart.Items)
                                {
                                    <div class="d-flex align-items-center mb-3 pb-3 border-bottom">
                                        <img src="https://raw.githubusercontent.com/MicrosoftDocs/mslearn-dotnet-cloudnative/main/dotnet-docker/Products/wwwroot/images/@item.Product?.ImageUrl" 
                                             alt="@item.Product?.Name" class="me-3" style="width: 60px; height: 60px; object-fit: cover;" />
                                        <div class="flex-grow-1">
                                            <h6 class="mb-0">@item.Product?.Name</h6>
                                            <small class="text-muted">Quantity: @item.Quantity</small>
                                        </div>
                                        <div class="text-end">
                                            <div>@item.UnitPrice.ToString("C") each</div>
                                            <strong>@item.TotalPrice.ToString("C")</strong>
                                        </div>
                                    </div>
                                }
                            </div>
                            
                            <div class="d-flex justify-content-between">
                                <button class="btn btn-outline-secondary" @onclick="() => currentStep = 1">
                                    <i class="fas fa-arrow-left"></i> Edit Information
                                </button>
                                <button class="btn btn-success" @onclick="PlaceOrder" disabled="@isProcessingOrder">
                                    @if (isProcessingOrder)
                                    {
                                        <span class="spinner-border spinner-border-sm me-2"></span>
                                    }
                                    Place Order
                                </button>
                            </div>
                        </div>
                    </div>
                }
            </div>
            
            <!-- Order Summary Sidebar -->
            <div class="col-lg-4">
                <div class="card">
                    <div class="card-header">
                        <h5>Order Summary</h5>
                    </div>
                    <div class="card-body">
                        <div class="summary-row">
                            <span>Subtotal (@cart.TotalItems items):</span>
                            <span>@cart.TotalAmount.ToString("C")</span>
                        </div>
                        <div class="summary-row">
                            <span>Shipping:</span>
                            <span class="text-success">FREE</span>
                        </div>
                        <div class="summary-row">
                            <span>Tax:</span>
                            <span>$0.00</span>
                        </div>
                        <hr />
                        <div class="summary-row total">
                            <strong>
                                <span>Total:</span>
                                <span>@cart.TotalAmount.ToString("C")</span>
                            </strong>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
</div>

@code {
    private Cart? cart;
    private Customer customer = new();
    private int currentStep = 1;
    private bool isProcessingOrder = false;
    
    protected override async Task OnInitializedAsync()
    {
        cart = await CartService.GetCartAsync();
        if (cart == null || !cart.Items.Any())
        {
            return;
        }
    }
    
    private async Task ProceedToReview()
    {
        if (await CheckoutService.ValidateCustomerAsync(customer))
        {
            currentStep = 2;
        }
    }
    
    private async Task PlaceOrder()
    {
        isProcessingOrder = true;
        try
        {
            var order = await CheckoutService.CreateOrderAsync(cart!, customer);
            await CartService.ClearCartAsync();
            Navigation.NavigateTo($"/order-confirmation/{order.OrderId}");
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error placing order: {ex.Message}");
        }
        finally
        {
            isProcessingOrder = false;
        }
    }
}

<style>
    .checkout-progress {
        display: flex;
        justify-content: space-between;
        margin-bottom: 2rem;
    }
    
    .progress-step {
        display: flex;
        flex-direction: column;
        align-items: center;
        flex: 1;
        position: relative;
    }
    
    .progress-step:not(:last-child)::after {
        content: '';
        position: absolute;
        top: 20px;
        left: 60%;
        width: 80%;
        height: 2px;
        background-color: #ddd;
        z-index: 1;
    }
    
    .progress-step.active:not(:last-child)::after {
        background-color: #007bff;
    }
    
    .step-number {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background-color: #ddd;
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: bold;
        margin-bottom: 8px;
        z-index: 2;
        position: relative;
    }
    
    .progress-step.active .step-number {
        background-color: #007bff;
    }
    
    .step-label {
        font-size: 14px;
        font-weight: 500;
    }
    
    .summary-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 10px;
    }
    
    .summary-row.total {
        font-size: 1.2em;
        margin-bottom: 0;
    }
    
    .customer-info-review {
        background: #f8f9fa;
        padding: 15px;
        border-radius: 5px;
    }
</style>
```

**Components/Pages/OrderConfirmation.razor:**
```aspnetcorerazor
@page "/order-confirmation/{OrderId}"
@using CartEntities
@inject NavigationManager Navigation

<PageTitle>Order Confirmation</PageTitle>

<div class="container mt-4">
    <div class="row justify-content-center">
        <div class="col-lg-8">
            <div class="text-center mb-4">
                <div class="success-icon mb-3">
                    <i class="fas fa-check-circle fa-5x text-success"></i>
                </div>
                <h1 class="text-success">Order Confirmed!</h1>
                <p class="lead">Thank you for your purchase. Your order has been received and is being processed.</p>
            </div>
            
            <div class="card">
                <div class="card-header">
                    <h5>Order Details</h5>
                </div>
                <div class="card-body">
                    <div class="row mb-3">
                        <div class="col-sm-3"><strong>Order Number:</strong></div>
                        <div class="col-sm-9">@OrderId</div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-sm-3"><strong>Order Date:</strong></div>
                        <div class="col-sm-9">@DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt")</div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-sm-3"><strong>Status:</strong></div>
                        <div class="col-sm-9">
                            <span class="badge bg-warning">Pending</span>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="card mt-4">
                <div class="card-header">
                    <h5>What's Next?</h5>
                </div>
                <div class="card-body">
                    <ul class="list-unstyled">
                        <li class="mb-2">
                            <i class="fas fa-envelope text-primary me-2"></i>
                            You will receive an email confirmation at the address provided
                        </li>
                        <li class="mb-2">
                            <i class="fas fa-box text-primary me-2"></i>
                            Your order will be processed and prepared for shipping
                        </li>
                        <li class="mb-2">
                            <i class="fas fa-truck text-primary me-2"></i>
                            You'll receive tracking information once your order ships
                        </li>
                    </ul>
                </div>
            </div>
            
            <div class="text-center mt-4">
                <a href="/products" class="btn btn-primary me-3">Continue Shopping</a>
                <button class="btn btn-outline-secondary" @onclick="PrintOrder">
                    <i class="fas fa-print"></i> Print Order
                </button>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public string OrderId { get; set; } = string.Empty;
    
    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(OrderId))
        {
            Navigation.NavigateTo("/");
        }
    }
    
    private void PrintOrder()
    {
        // Implement print functionality
        // For now, just log the action
        Console.WriteLine($"Print order: {OrderId}");
    }
}

<style>
    .success-icon {
        animation: pulse 2s infinite;
    }
    
    @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.05); }
        100% { transform: scale(1); }
    }
</style>
```

#### Step 3.3: Update Navigation and Layout

**Update Store/Components/Layout/NavMenu.razor to include cart summary:**
```aspnetcorerazor
<!-- Add this to the navbar -->
<div class="navbar-nav ms-auto">
    <CartSummary />
</div>
```

**Update Store/Components/Layout/MainLayout.razor to include required CSS:**
```html
<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
```

### 12.4 Phase 4: Integration & Testing (Week 6)

#### Step 4.1: Update Project References

**Update Store/Store.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\eShopServiceDefaults\eShopServiceDefaults.csproj" />
    <ProjectReference Include="..\DataEntities\DataEntities.csproj" />
    <ProjectReference Include="..\CartEntities\CartEntities.csproj" />
    <ProjectReference Include="..\SearchEntities\SearchEntities.csproj" />
  </ItemGroup>
</Project>
```

**Update Store/Program.cs:**
```csharp
using Store.Components;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// Add existing services
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

// Add cart and checkout services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

#### Step 4.2: Testing Checklist

**Unit Tests:**
- [ ] Cart service operations (add, remove, update, clear)
- [ ] Customer validation logic
- [ ] Order creation process
- [ ] Cart persistence in session storage

**Integration Tests:**
- [ ] End-to-end shopping flow
- [ ] Session management across page navigation
- [ ] Form validation and error handling
- [ ] Component communication and state management

**User Acceptance Tests:**
- [ ] Add products to cart from product listing
- [ ] View and modify cart contents
- [ ] Complete checkout process
- [ ] Receive order confirmation
- [ ] Cart persistence during session
- [ ] Cart clearing after order completion

**Performance Tests:**
- [ ] Cart operations under load
- [ ] Session storage performance
- [ ] Component rendering performance
- [ ] Memory usage optimization

### 12.5 Deployment and Go-Live

#### Step 5.1: Pre-deployment Checklist

**Code Quality:**
- [ ] All unit tests passing
- [ ] Code review completed
- [ ] Accessibility compliance verified
- [ ] Cross-browser testing completed
- [ ] Mobile responsiveness tested

**Configuration:**
- [ ] Production configuration settings
- [ ] Error handling and logging
- [ ] Performance optimization
- [ ] Security review completed

#### Step 5.2: Monitoring and Analytics

**Post-deployment Monitoring:**
- Cart abandonment rates
- Order completion rates
- Session storage usage
- Component performance metrics
- User journey analytics

**Success Metrics:**
- Shopping cart adoption rate > 80%
- Order completion rate > 90%
- Average cart value tracking
- User session duration increase

This comprehensive implementation guide provides all the necessary code, configurations, and processes needed to successfully add a fully functional shopping cart to the eShopLite application. Each phase builds systematically on the previous one, ensuring a robust and maintainable solution.
