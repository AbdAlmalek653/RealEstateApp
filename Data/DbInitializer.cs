using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Models;
using System;
using System.Threading.Tasks;

namespace RealEstateApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. إضافة دور SuperAdmin إلى قائمة الأدوار الأساسية
            string[] roleNames = { "SuperAdmin", "Admin", "Seller", "Buyer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء وتأكيد حساب الـ Super Admin
            var superAdminEmail = "moh1.g.w1@gmail.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                var newSuperAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = "مدير النظام (Super Admin)",
                    PhoneNumber = "0500000000",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newSuperAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    // إعطاؤه صلاحيات الـ SuperAdmin وكذلك Admin لضمان الوصول لكافة الأجزاء
                    await userManager.AddToRoleAsync(newSuperAdmin, "SuperAdmin");
                    await userManager.AddToRoleAsync(newSuperAdmin, "Admin");
                }
            }
            else
            {
                // في حال كان الحساب موجوداً مسبقاً، نضمن منحه دور SuperAdmin تلقائياً
                if (!await userManager.IsInRoleAsync(superAdminUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                }
            }
        }
    }
}