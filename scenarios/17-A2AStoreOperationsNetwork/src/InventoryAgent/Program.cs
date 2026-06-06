var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Inventory API endpoint
app.MapPost("/api/inventory/check", (InventoryCheckRequest request) =>
{
    // Simulate inventory checking logic
    var random = new Random();
    var stock = random.Next(0, 100);
    
    return Results.Ok(new InventoryCheckResponse 
    { 
        ProductId = request.ProductId, 
        Stock = stock 
    });
})
.WithName("CheckInventory")
.WithOpenApi();

app.Run();

// Request/Response models
public record InventoryCheckRequest(string ProductId);
public record InventoryCheckResponse
{
    public string ProductId { get; init; } = string.Empty;
    public int Stock { get; init; }
}
