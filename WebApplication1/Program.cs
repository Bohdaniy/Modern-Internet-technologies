using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Repositories;
using WebApplicationData.Models.Configurations;

var builder = WebApplication.CreateBuilder(args);

// --- 1. НАЛАШТУВАННЯ КОНФІГУРАЦІЇ (ЗАВДАННЯ 1) ---
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("sharedsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Завантаження секретів лише в середовищі розробки
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// --- 2. СТРОГО ТИПІЗОВАНА КОНФІГУРАЦІЯ ---
var myConfig = builder.Configuration.Get<MyConfiguration>();
if (myConfig == null)
{
    throw new InvalidOperationException("Configuration object 'MyConfiguration' could not be loaded.");
}
builder.Services.AddSingleton(myConfig);

// --- 3. НАЛАШТУВАННЯ СЕРВІСІВ ---
var connectionString = myConfig.ConnectionStrings?.DefaultConnection
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

builder.Services.AddDbContext<WebApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<WebApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<WebApplicationDbContext>();

builder.Services.AddScoped<IWebRepository, WebRepository>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// --- 4. НАЛАШТУВАННЯ RATE LIMITING (ЗАВДАННЯ 6) ---
builder.Services.AddRateLimiter(options =>
{
    // Відповідь при перевищенні ліміту
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
    };

    // Глобальна політика для всіх запитів
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.Identity?.Name ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"user:{userId}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100, // 100 запитів на хвилину для користувачів
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }
        else
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"ip:{ip}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5, // 5 запитів на хвилину для гостей (для тесту)
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }
    });
});

var app = builder.Build();

// --- 5. НАЛАШТУВАННЯ PIPELINE ---
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRateLimiter(); // 👈 Обов'язково перед UseRouting

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
