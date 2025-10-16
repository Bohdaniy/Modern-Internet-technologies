using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Додавання сервісів у контейнер DI
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Реєстрація DbContext
builder.Services.AddDbContext<WebApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Для зручності розробки
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Реєстрація Identity з кастомним користувачем WebApplicationUser
builder.Services.AddDefaultIdentity<WebApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
})
.AddEntityFrameworkStores<WebApplicationDbContext>();

// Реєстрація власних репозиторіїв
builder.Services.AddScoped<IWebRepository, WebRepository>();

// Додавання MVC та Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 2. Налаштування HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Для автоматичного застосування міграцій у Dev
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HSTS для продакшн
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Для wwwroot

app.UseRouting();

// **Обов’язково спочатку аутентифікація, потім авторизація**
app.UseAuthentication();
app.UseAuthorization();

// Маршрути для контролерів та Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
