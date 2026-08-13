using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using RealEstateApp.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var property = new Property
                {
                    Title = model.Title,
                    Governorate = model.Governorate,
                    City = model.City,
                    Area = model.Area,
                    RoomsCount = model.RoomsCount,
                    LegalStatus = model.LegalStatus,
                    PhoneDialCode = model.PhoneDialCode,
                    PhoneNumber = model.PhoneNumber,
                    Description = model.Description,
                    Price = model.Price,
                    SellerId = _userManager.GetUserId(User) ?? string.Empty
                };

                // معالجة رفع الصور إن وجدت...
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    foreach (var file in model.ImageFiles)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        property.Images.Add(new PropertyImage { ImagePath = "/uploads/" + uniqueFileName });
                    }
                }

                _context.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}