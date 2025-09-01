using Microsoft.EntityFrameworkCore;
using PaymentsService.Components;
using PaymentsService.Data;
using PaymentsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - includes telemetry, health checks, and service discovery
builder.AddServiceDefaults();

// Add Entity Framework with SQLite for local development
builder.Services.AddDbContext<PaymentsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PaymentsDb") 
        ?? "Data Source=Data/payments.db";
    options.UseSqlite(connectionString);
});

// Register payment repository
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Aspire health check and telemetry endpoints
app.MapDefaultEndpoints();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Map API controllers
app.MapControllers();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
