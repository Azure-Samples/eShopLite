# Product API Endpoints

## Overview

The Product API provides comprehensive REST endpoints for product management, search operations, and AI-powered semantic search. The API follows RESTful conventions and includes both traditional CRUD operations and advanced AI-driven search capabilities.

## API Endpoint Overview

### Standard Product Operations

#### GET /api/Product/
Retrieves all products from the database.

**Response**: 200 OK with array of products
```json
[
  {
    "id": 1,
    "name": "Mountain Hiking Backpack",
    "description": "Durable 40L backpack for outdoor adventures",
    "price": 129.99,
    "brand": "OutdoorGear",
    "category": "Backpacks"
  }
]
```

#### GET /api/Product/{id}
Retrieves a specific product by ID.

**Parameters**: 
- `id` (int): Product identifier

**Response**: 
- 200 OK with product details
- 404 Not Found if product doesn't exist

#### POST /api/Product/
Creates a new product.

**Request Body**:
```json
{
  "name": "New Product",
  "description": "Product description",
  "price": 99.99,
  "brand": "Brand Name",
  "category": "Category"
}
```

**Response**: 201 Created with created product

#### PUT /api/Product/{id}
Updates an existing product.

**Parameters**: 
- `id` (int): Product identifier

**Request Body**: Complete product object

**Response**: 
- 200 OK if updated successfully
- 404 Not Found if product doesn't exist

#### DELETE /api/Product/{id}
Deletes a product by ID.

**Parameters**: 
- `id` (int): Product identifier

**Response**: 
- 200 OK if deleted successfully
- 404 Not Found if product doesn't exist

### Search Operations

#### GET /api/Product/search/{search}
Performs traditional keyword-based search on product names.

**Parameters**: 
- `search` (string): Search term

**Response**: 200 OK with search results and metadata
```json
{
  "query": "hiking",
  "products": [
    {
      "id": 1,
      "name": "Mountain Hiking Backpack",
      "description": "Durable 40L backpack for outdoor adventures",
      "price": 129.99,
      "brand": "OutdoorGear",
      "category": "Backpacks"
    }
  ],
  "metadata": {
    "totalResults": 1,
    "searchType": "keyword"
  }
}
```

#### GET /api/aisearch/{search}
Performs AI-powered semantic search using vector embeddings.

**Parameters**: 
- `search` (string): Natural language search query

**Response**: 
- 200 OK with semantic search results
- 404 Not Found if search service unavailable

```json
{
  "query": "gear for mountain climbing",
  "products": [
    {
      "id": 1,
      "name": "Mountain Hiking Backpack",
      "description": "Durable 40L backpack for outdoor adventures",
      "price": 129.99,
      "brand": "OutdoorGear",
      "category": "Backpacks"
    },
    {
      "id": 5,
      "name": "Climbing Rope 60m",
      "description": "Dynamic rope for rock climbing",
      "price": 199.99,
      "brand": "ClimbSafe",
      "category": "Climbing Gear"
    }
  ],
  "message": "Found 2 similar products",
  "isSuccessful": true
}
```

## Implementation Details

### Endpoint Configuration
The API endpoints are configured using ASP.NET Core minimal APIs:

```csharp
public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/Product").WithTags(nameof(Product));

    // Standard CRUD operations
    group.MapGet("/", ProductApiActions.GetAllProducts)
        .WithName("GetProducts")
        .WithOpenApi();

    group.MapGet("/{id}", ProductApiActions.GetProductById)
        .WithName("GetProductById")
        .WithOpenApi();

    group.MapPost("/", ProductApiActions.CreateProduct)
        .WithName("CreateProduct")
        .WithOpenApi();

    group.MapPut("/{id}", ProductApiActions.UpdateProduct)
        .WithName("UpdateProduct")
        .WithOpenApi();

    group.MapDelete("/{id}", ProductApiActions.DeleteProduct)
        .WithName("DeleteProduct")
        .WithOpenApi();

    // Search operations
    group.MapGet("/search/{search}", ProductApiActions.SearchProducts)
        .WithName("SearchProducts")
        .WithOpenApi();

    // AI-powered semantic search
    routes.MapGet("/api/aisearch/{search}", ProductAiActions.AISearch)
        .WithName("AISearch")
        .WithOpenApi();
}
```

### API Action Implementations

#### Standard Product Operations
```csharp
public static class ProductApiActions
{
    public static async Task<IResult> GetAllProducts(Context db)
    {
        var products = await db.Product.ToListAsync();
        return Results.Ok(products);
    }

    public static async Task<IResult> GetProductById(int id, Context db)
    {
        var product = await db.Product.FindAsync(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }

    public static async Task<IResult> CreateProduct(Product product, Context db)
    {
        db.Product.Add(product);
        await db.SaveChangesAsync();
        return Results.Created($"/api/Product/{product.Id}", product);
    }

    public static async Task<IResult> UpdateProduct(int id, Product inputProduct, Context db)
    {
        var product = await db.Product.FindAsync(id);
        if (product is null) return Results.NotFound();

        product.Name = inputProduct.Name;
        product.Description = inputProduct.Description;
        product.Price = inputProduct.Price;
        product.Brand = inputProduct.Brand;
        product.Category = inputProduct.Category;

        await db.SaveChangesAsync();
        return Results.Ok(product);
    }

    public static async Task<IResult> DeleteProduct(int id, Context db)
    {
        var product = await db.Product.FindAsync(id);
        if (product is null) return Results.NotFound();

        db.Product.Remove(product);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> SearchProducts(string search, Context db)
    {
        var products = await db.Product
            .Where(p => p.Name.Contains(search))
            .ToListAsync();

        var result = new
        {
            Query = search,
            Products = products,
            Metadata = new
            {
                TotalResults = products.Count,
                SearchType = "keyword"
            }
        };

        return Results.Ok(result);
    }
}
```

#### AI-Powered Search Actions
```csharp
public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, MemoryContext mc)
    {
        var result = await mc.Search(search, db);
        return Results.Ok(result);
    }
}
```

## Error Handling

### Global Exception Handling
The API includes comprehensive error handling:

```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        var response = new
        {
            error = "An error occurred while processing your request.",
            details = exception?.Message
        };

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(response);
    });
});
```

### Validation
Input validation is handled through model validation:

```csharp
public class Product
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
}
```

## OpenAPI Documentation

### Swagger Integration
The API includes OpenAPI documentation:

```csharp
builder.Services.AddOpenApi();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Product API v1");
});
```

### API Documentation Features
- Interactive API explorer
- Request/response schemas
- Example payloads
- Authentication requirements
- Error response codes

## Performance Considerations

### Database Optimization
- Entity Framework tracking optimization
- Efficient query patterns
- Connection pooling

### Caching Strategy
```csharp
builder.Services.AddMemoryCache();

// Cache frequently accessed products
public static async Task<IResult> GetProductById(int id, Context db, IMemoryCache cache)
{
    var cacheKey = $"product_{id}";
    
    if (cache.TryGetValue(cacheKey, out Product? cachedProduct))
    {
        return Results.Ok(cachedProduct);
    }

    var product = await db.Product.FindAsync(id);
    if (product is not null)
    {
        cache.Set(cacheKey, product, TimeSpan.FromMinutes(5));
    }

    return product is not null ? Results.Ok(product) : Results.NotFound();
}
```

### Rate Limiting
Implement rate limiting for API protection:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});
```

## Security

### Authentication
The API can be secured with various authentication schemes:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-auth-server";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });
```

### Authorization
Role-based access control:

```csharp
group.MapPost("/", ProductApiActions.CreateProduct)
    .RequireAuthorization("AdminOnly")
    .WithName("CreateProduct");

group.MapDelete("/{id}", ProductApiActions.DeleteProduct)
    .RequireAuthorization("AdminOnly")
    .WithName("DeleteProduct");
```

## Testing

### Unit Testing
Example unit test for API endpoints:

```csharp
[Test]
public async Task GetProductById_ReturnsProduct_WhenProductExists()
{
    // Arrange
    var options = new DbContextOptionsBuilder<Context>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    using var context = new Context(options);
    var product = new Product { Id = 1, Name = "Test Product" };
    context.Product.Add(product);
    await context.SaveChangesAsync();

    // Act
    var result = await ProductApiActions.GetProductById(1, context);

    // Assert
    Assert.That(result, Is.InstanceOf<Results<Ok<Product>, NotFound>>());
}
```

### Integration Testing
Full API integration tests:

```csharp
[Test]
public async Task SearchProducts_ReturnsResults_WhenProductsMatch()
{
    // Arrange
    var application = new WebApplicationFactory<Program>();
    var client = application.CreateClient();

    // Act
    var response = await client.GetAsync("/api/Product/search/hiking");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.That(content, Contains.Substring("hiking"));
}
```