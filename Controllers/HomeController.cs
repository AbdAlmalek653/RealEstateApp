using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // جلب أحدث العقارات مع الصور والبائع لعرضها في الصفحة الرئيسية
            var latestProperties = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6) // عرض أحدث 6 عقارات فقط في الهوم
                .ToListAsync();

            return View(latestProperties);
        }
    }
}