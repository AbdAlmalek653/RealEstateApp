using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Models;

namespace RealEstateApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyContactLog> PropertyContactLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. علاقة العقار بالبائع / الناشر
            builder.Entity<Property>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. علاقة العقار بالأدمن المسؤول (في حال حذف الأدمن لا يُحذف العقار بل يصبح null)
            builder.Entity<Property>()
                .HasOne(p => p.AssignedAdmin)
                .WithMany()
                .HasForeignKey(p => p.AssignedAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // 3. علاقة صور العقار بالعقار (حذف العقار يحذف صوره تلقائياً)
            builder.Entity<PropertyImage>()
                .HasOne(i => i.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. علاقة سجل النقرات بالعقار وبالأدمن
            builder.Entity<PropertyContactLog>()
                .HasOne(l => l.Property)
                .WithMany()
                .HasForeignKey(l => l.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PropertyContactLog>()
                .HasOne(l => l.Admin)
                .WithMany()
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}