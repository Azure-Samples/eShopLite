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

// Research API endpoint
app.MapPost("/api/researcher/insights", (ResearchRequest request) =>
{
    // Simulate research insights generation
    var random = new Random();
    var insights = new List<Insight>
    {
        new Insight 
        { 
            Review = $"Great product for outdoor activities! Product ID: {request.ProductId}", 
            Rating = 4.0 + random.NextDouble() 
        },
        new Insight 
        { 
            Review = "Excellent quality and durable materials.", 
            Rating = 4.2 + random.NextDouble() * 0.8 
        }
    };
    
    return Results.Ok(new ResearchResponse 
    { 
        ProductId = request.ProductId, 
        Insights = insights 
    });
})
.WithName("GetProductInsights")
.WithOpenApi();

app.Run();

// Request/Response models
public record ResearchRequest(string ProductId);
public record ResearchResponse
{
    public string ProductId { get; init; } = string.Empty;
    public List<Insight> Insights { get; init; } = new();
}

public record Insight
{
    public string Review { get; init; } = string.Empty;
    public double Rating { get; init; }
}
