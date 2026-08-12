using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealEstateApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // استخدام نفس نص الاتصال بـ SQLite
            optionsBuilder.UseSqlite("Data Source=realestate.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}