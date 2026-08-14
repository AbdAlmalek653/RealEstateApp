using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
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

        [HttpGet]
        public async Task<IActionResult> Index(
     string? searchQuery,
     string? governorate,
     string? legalStatus,
     decimal? minPrice,
     decimal? maxPrice)
        {
            IQueryable<Property> query = _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();

                query = query.Where(p =>
                    p.Title.Contains(searchQuery) ||
                    p.Governorate.Contains(searchQuery) ||
                    p.City.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(governorate))
            {
                governorate = governorate.Trim();
                query = query.Where(p => p.Governorate == governorate);
            }

            if (!string.IsNullOrWhiteSpace(legalStatus))
            {
                legalStatus = legalStatus.Trim();
                query = query.Where(p => p.LegalStatus == legalStatus);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }

    }
}