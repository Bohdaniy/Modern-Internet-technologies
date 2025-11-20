using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using WebApplication1.Authorization;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Models.Configurations;
using WebApplicationData.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. НАЛАШТУВАННЯ КОНФІГУРАЦІЇ (ЗАВДАННЯ 1 з ЛР 3) ---
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("sharedsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Завантаження секретів лише в середовищі розробки (Завдання 4 з ЛР 3)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// --- 2. СТРОГО ТИПІЗОВАНА КОНФІГУРАЦІЯ (ЗАВДАННЯ 3 з ЛР 3) ---
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

// Реєстрація репозиторію (з ЛР 2)
builder.Services.AddScoped<IWebRepository, WebRepository>();

// --- 4. НАЛАШТУВАННЯ RATE LIMITING (ЗАВДАННЯ 6 з ЛР 3) ---
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
                    PermitLimit = 20, // 20 запитів на хвилину для гостей
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }
    });
});

// --- 5. НАЛАШТУВАННЯ АВТОРИЗАЦІЇ (ЛР 4) ---

// Реєстрація обробників (Handlers)
builder.Services.AddSingleton<IAuthorizationHandler, IsAuthorHandler>();       // Завдання 3 з ЛР 4
builder.Services.AddSingleton<IAuthorizationHandler, MinHoursHandler>();       // Завдання 4 з ЛР 4
builder.Services.AddSingleton<IAuthorizationHandler, ForumAccessHandler>();    // Завдання 5 з ЛР 4

// Реєстрація політик
builder.Services.AddAuthorization(options =>
{
    // ЛАБ 4 | Завдання 2
    options.AddPolicy("ArchivePolicy", policy =>
        policy.RequireClaim("IsVerifiedClient"));

    // ЛАБ 4 | Завдання 3 (Ресурсна авторизація)
    options.AddPolicy("ResourceOwner", policy =>
        policy.Requirements.Add(new IsAuthorRequirement()));

    // ЛАБ 4 | Завдання 4 (Вимога годин)
    options.AddPolicy("PremiumContent", policy =>
        policy.Requirements.Add(new MinHoursRequirement(100)));

    // ЛАБ 4 | Завдання 5 (OR логіка для форуму)
    options.AddPolicy("ForumPolicy", policy =>
        policy.Requirements.Add(new ForumAccessRequirement()));
});


//  НАЛАШТУВАННЯ ЛОКАЛІЗАЦІЇ (ЛР 5) 

// 1. Реєстрація сервісів локалізації
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. Налаштування списку культур для сервісів (щоб працював SelectLanguagePartial)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"), // Англійська (США)
        new CultureInfo("uk-UA"), // Українська
        new CultureInfo("es")     // Іспанська
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// 3. Додавання локалізації до контролерів та View
// + Глобальний фільтр авторизації з ЛР 4
builder.Services.AddControllersWithViews(options =>
{
    // ЛАБ 4 | Завдання 1: Закриття доступу до контролерів за замовчуванням
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization();


var app = builder.Build();


// --- 7. НАЛАШТУВАННЯ КОНВЕЄРА (PIPELINE) ---

// [ЛАБ 5] Middleware локалізації має бути одним з перших
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRateLimiter();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages()
    .RequireRateLimiting("PartitionedPolicy") // Застосовуємо ліміт, якщо треба
    .AllowAnonymous();

app.Run();