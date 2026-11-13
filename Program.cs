using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Lưu DataProtection keys trong thư mục nội bộ dự án để đảm bảo tính ổn định của Cookie session
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "obj", "dp_keys");
if (!Directory.Exists(keysFolder))
{
    Directory.CreateDirectory(keysFolder);
}
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("NMCNPM");

// 2. Yêu cầu xác thực (đăng nhập) cho toàn bộ ứng dụng, ngoại trừ các action có [AllowAnonymous]
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddRazorOptions(options =>
{
    // Cho phép Razor tìm View ở cả thư mục /Account và /Views/Account
    options.ViewLocationFormats.Add("/Account/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Account/{0}.cshtml");
});

// 3. Cấu hình DbContext linh hoạt cho SQL Server và SQLite
var dbProvider = builder.Configuration.GetValue<string>("DbProvider") ?? "SqlServer";

builder.Services.AddDbContext<HrmDbContext>(options =>
{
    switch (dbProvider.ToLower())
    {
        case "sqlite":
            var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") 
                          ?? "Data Source=db_HRM_Tocotoco.db";
            options.UseSqlite(sqliteConn);
            break;

        case "sqlserver":
        default:
            var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection") 
                             ?? builder.Configuration.GetConnectionString("SqlServerConnection");
            options.UseSqlServer(sqlServerConn);
            break;
    }
});

// 4. Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// 5. Cấu hình HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();