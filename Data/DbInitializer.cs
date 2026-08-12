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

            // 1. إنشاء الأدوار الأساسية
            string[] roleNames = { "Admin", "Seller", "Buyer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء حساب الأدمن الافتراضي
            var adminEmail = "admin@realestate.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير النظام",
                    PhoneNumber = "0500000000",
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}