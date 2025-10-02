using ShoppingAssistantAgent.Tools;
using ShoppingAssistantAgent.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP client for Products API
builder.Services.AddHttpClient("products", client =>
{
    // Will be configured via Aspire service discovery
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Agent Tools
builder.Services.AddSingleton<SearchCatalogTool>();
builder.Services.AddSingleton<ProductDetailsTool>();
builder.Services.AddSingleton<AddToCartTool>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Chat endpoint
app.MapPost("/api/agent/chat", async (
    ChatRequest request,
    SearchCatalogTool searchTool,
    ProductDetailsTool detailsTool,
    AddToCartTool cartTool,
    ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Received chat message: {Message}", request.Message);

        // Simple response for now - will be enhanced with Agent Framework integration
        var response = new ChatResponse
        {
            Message = $"I received your message: {request.Message}. The Shopping Assistant is ready to help!",
            ConversationId = request.ConversationId ?? Guid.NewGuid().ToString()
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing chat request");
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("AgentChat")
.WithOpenApi();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
