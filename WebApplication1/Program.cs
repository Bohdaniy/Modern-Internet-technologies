using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. ��������� ������ � ��������� DI
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ��������� DbContext
builder.Services.AddDbContext<WebApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ��� �������� ��������
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ��������� Identity � ��������� ������������ WebApplicationUser
builder.Services.AddDefaultIdentity<WebApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
})
.AddEntityFrameworkStores<WebApplicationDbContext>();

// ��������� ������� ����������
builder.Services.AddScoped<IWebRepository, WebRepository>();

// ��������� MVC �� Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 2. ������������ HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // ��� ������������� ������������ ������� � Dev
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HSTS ��� ��������
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ��� wwwroot

app.UseRouting();

// **���������� �������� ��������������, ���� �����������**
app.UseAuthentication();
app.UseAuthorization();

// �������� ��� ���������� �� Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
