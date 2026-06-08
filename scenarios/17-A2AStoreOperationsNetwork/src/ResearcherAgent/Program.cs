var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// ── A2A Agent Card ─────────────────────────────────────────────────────────────────────
// Exposes agent metadata at the well-known A2A discovery URL so that orchestrators
// can identify this agent's capabilities and skills via a standard GET request.
// Schema mirrors the A2A protocol AgentCard specification.
// We map both the current ("agent-card.json") and legacy ("agent.json") well-known paths.
IResult BuildAgentCard(HttpContext ctx)
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Json(new
    {
        name        = "Researcher Agent",
        description = "Generates product reviews and quality insights for eShop products.",
        version     = "1.0.0",
        url         = baseUrl,
        supportedInterfaces = new[]
        {
            new { url = $"{baseUrl}/api/researcher/insights", protocolBinding = "HTTP+JSON", protocolVersion = "1.0" }
        },
        defaultInputModes  = new[] { "application/json" },
        defaultOutputModes = new[] { "application/json" },
        capabilities = new { streaming = false },
        skills = new[]
        {
            new
            {
                id          = "get_product_insights",
                name        = "Get Product Insights",
                description = "Returns synthesised customer reviews and star ratings for a given product ID.",
                tags        = new[] { "insights", "reviews", "ratings", "research" }
            }
        }
    });
}
app.MapGet("/.well-known/agent-card.json", BuildAgentCard).WithName("GetAgentCard");
app.MapGet("/.well-known/agent.json", BuildAgentCard).WithName("GetAgentCardLegacy");

// ── Research API endpoint ──────────────────────────────────────────────────────────────
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
.WithName("GetProductInsights");

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
