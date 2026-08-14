using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealEstateApp.Data;
using RealEstateApp.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. تحديد مسار قاعدة البيانات SQLite وإنشاء مجلد App_Data تلقائياً إذا لم يكن موجوداً
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
if (!Directory.Exists(appDataPath))
{
    Directory.CreateDirectory(appDataPath);
}

var dbPath = Path.Combine(appDataPath, "realestate.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. إعداد Identity والأدوار وإلغاء تعقيدات كلمات المرور لتسهيل التطوير
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. ضبط مسارات الدخول وملفات تعريف الارتباط (Cookies)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. معالجة البيئة ومسار معالجة الطلبات (Middleware Pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 5. تهيئة وتحديث قاعدة البيانات دون حذف البيانات السابقة
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        // استخدام EnsureCreatedAsync لتطبيق الجداول دون مسح البيانات السابقة
        await context.Database.MigrateAsync();


        // إنشاء الأدوار وحساب السوبر أدمن إذا لم تكن موجودة مسبقاً
        await DbInitializer.SeedAsync(services);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "حدث خطأ أثناء تهيئة قاعدة البيانات.");
}

app.Run();