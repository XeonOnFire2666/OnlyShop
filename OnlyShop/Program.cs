using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using OnlyShop.Adapters;
using OnlyShop.Application;
using OnlyShop.Infrastructure;
using OnlyShop.Infrastructure.Jwt;
using OnlyShop.Infrastructure.Persistence;
using OnlyShop.Adapters.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5201");
var services = builder.Services;

services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

services.AddApplication(builder.Configuration);
services.AddInfrastructure(builder.Configuration);
services.AddAdapters(builder.Configuration);

services.AddDbContext<OnlyShopDbContext>();
services.AddAuthorization();
services.AddOpenApi();
services.AddControllers();
services.AddMemoryCache();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<OnlyShopDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();