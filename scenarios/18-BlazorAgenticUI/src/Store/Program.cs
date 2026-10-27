using Store.Components;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient("agent", client =>
{
    client.BaseAddress = new Uri("https+http://agent");
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<CatalogClient>(client => client.BaseAddress = new Uri("https+http://products"));
builder.Services.AddScoped<CartState>();
var app = builder.Build();
app.UseExceptionHandler("/error");
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public partial class Program;
