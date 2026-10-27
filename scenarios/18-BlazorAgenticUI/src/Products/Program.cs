using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Products;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
var demo = builder.Configuration.GetValue<bool>("Demo");
builder.Services.AddDbContext<CatalogDb>(options =>
{
    if (demo) { options.UseSqlite(builder.Configuration.GetConnectionString("DemoCatalog") ?? "Data Source=catalog-demo.db"); }
    else { options.UseSqlServer(builder.Configuration.GetConnectionString("productsDb")); }
});
IEmbeddingGenerator<string, Embedding<float>>? embeddings = demo ? null :
    builder.Configuration.CreateModelClient()
        .GetEmbeddingClient(builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-3-small")
        .AsIEmbeddingGenerator();
builder.Services.AddSingleton(new CatalogSearch(embeddings));
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapGet("/api/product", async (CatalogDb db, CancellationToken ct) =>
    await db.Products.AsNoTracking().OrderBy(product => product.Id).Take(SearchInput.MaximumResults).ToArrayAsync(ct));
app.MapGet("/api/product/{id:int}", async (int id, CatalogDb db, CancellationToken ct) =>
    await db.Products.FindAsync([id], ct) is { } product ? Results.Ok(product) : Results.NotFound());
app.MapGet("/api/product/search", async (string query, decimal? maxPrice, int? limit, bool? semantic,
    CatalogDb db, CatalogSearch search, CancellationToken ct) =>
{
    try { SearchInput.Validate(query, maxPrice, limit ?? 3); }
    catch (ArgumentException) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["query"] = ["Invalid search text, price, or result limit."] }); }
    if (semantic == true && demo)
    {
        return Results.Problem("Semantic search is unavailable in explicit demo mode.", statusCode: 503);
    }
    var products = await db.Products.AsNoTracking().ToArrayAsync(ct);
    return Results.Ok(await search.SearchAsync(products, query, maxPrice, limit ?? 3, semantic == true, ct));
});
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDb>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Products.AnyAsync())
    {
        if (db.Database.IsSqlServer())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Products] ON");
            db.Products.AddRange(CatalogSeed.Products);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Products] OFF");
            await transaction.CommitAsync();
        }
        else
        {
            db.Products.AddRange(CatalogSeed.Products);
            await db.SaveChangesAsync();
        }
    }
    await app.Services.GetRequiredService<CatalogSearch>().InitializeAsync(await db.Products.AsNoTracking().ToArrayAsync(), CancellationToken.None);
}
app.Run();

public partial class Program;
