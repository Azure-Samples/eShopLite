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

// Promotions API endpoint
app.MapPost("/api/promotions/active", (PromotionsRequest request) =>
{
    // Simulate promotion checking logic
    var random = new Random();
    var hasPromotion = random.Next(0, 100) < 30; // 30% chance of promotion
    
    var promotions = new List<Promotion>();
    if (hasPromotion)
    {
        promotions.Add(new Promotion 
        { 
            Title = "Special Offer", 
            Discount = random.Next(5, 50) 
        });
    }
    
    return Results.Ok(new PromotionsResponse 
    { 
        ProductId = request.ProductId, 
        Promotions = promotions 
    });
})
.WithName("GetActivePromotions")
.WithOpenApi();

app.Run();

// Request/Response models
public record PromotionsRequest(string ProductId);
public record PromotionsResponse
{
    public string ProductId { get; init; } = string.Empty;
    public List<Promotion> Promotions { get; init; } = new();
}

public record Promotion
{
    public string Title { get; init; } = string.Empty;
    public int Discount { get; init; }
}
