var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// ── A2A Agent Card ─────────────────────────────────────────────────────────────────────
// Exposes agent metadata at the well-known A2A discovery URL so that orchestrators
// (and the Aspire dashboard) can identify this agent's capabilities without prior
// knowledge of its endpoints. Schema mirrors the A2A protocol AgentCard specification.
// We map both the current ("agent-card.json") and legacy ("agent.json") well-known paths.
IResult BuildAgentCard(HttpContext ctx)
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Json(new
    {
        name        = "Promotions Agent",
        description = "Returns active discount promotions for eShop products.",
        version     = "1.0.0",
        url         = baseUrl,
        supportedInterfaces = new[]
        {
            new { url = $"{baseUrl}/api/promotions/active", protocolBinding = "HTTP+JSON", protocolVersion = "1.0" }
        },
        defaultInputModes  = new[] { "application/json" },
        defaultOutputModes = new[] { "application/json" },
        capabilities = new { streaming = false },
        skills = new[]
        {
            new
            {
                id          = "get_active_promotions",
                name        = "Get Active Promotions",
                description = "Returns any active promotions/discounts for a given product ID (30% chance of a promotion).",
                tags        = new[] { "promotions", "discounts", "offers", "sales" }
            }
        }
    });
}
app.MapGet("/.well-known/agent-card.json", BuildAgentCard).WithName("GetAgentCard");
app.MapGet("/.well-known/agent.json", BuildAgentCard).WithName("GetAgentCardLegacy");

// ── Promotions API endpoint ────────────────────────────────────────────────────────────
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
.WithName("GetActivePromotions");

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
