var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// ── A2A Agent Card ─────────────────────────────────────────────────────────────────────
// The A2A protocol specifies that every agent MUST expose its metadata (capabilities,
// skills, endpoint URL) at /.well-known/agent-card.json so that orchestrators can discover it.
// We serve a hand-authored JSON object that mirrors the AgentCard schema from the
// A2A protocol specification (https://a2a-protocol.org/).
// Note: the A2A.AspNetCore package's MapWellKnownAgentCard() helper does the same thing
// under the hood; we use MapGet here because the agent projects don't yet take a
// dependency on A2A.AspNetCore (the existing REST endpoints keep working as-is).
// We map both the current ("agent-card.json") and legacy ("agent.json") well-known paths.
IResult BuildAgentCard(HttpContext ctx)
{
    // Derive the base URL from the live request so this works on any port Aspire assigns
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Json(new
    {
        name        = "Inventory Agent",
        description = "Provides real-time inventory stock levels for eShop products.",
        version     = "1.0.0",
        url         = baseUrl,
        // The HTTP endpoint where A2A messages (JSON-RPC) can be sent
        supportedInterfaces = new[]
        {
            new { url = $"{baseUrl}/api/inventory/check", protocolBinding = "HTTP+JSON", protocolVersion = "1.0" }
        },
        defaultInputModes  = new[] { "application/json" },
        defaultOutputModes = new[] { "application/json" },
        capabilities = new { streaming = false },
        skills = new[]
        {
            new
            {
                id          = "check_inventory",
                name        = "Check Inventory",
                description = "Returns the current stock level (0-100) for a given product ID.",
                tags        = new[] { "inventory", "stock", "availability" }
            }
        }
    });
}
app.MapGet("/.well-known/agent-card.json", BuildAgentCard).WithName("GetAgentCard");
app.MapGet("/.well-known/agent.json", BuildAgentCard).WithName("GetAgentCardLegacy");

// ── Inventory API endpoint ─────────────────────────────────────────────────────────────
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
.WithName("CheckInventory");

app.Run();

// Request/Response models
public record InventoryCheckRequest(string ProductId);
public record InventoryCheckResponse
{
    public string ProductId { get; init; } = string.Empty;
    public int Stock { get; init; }
}
