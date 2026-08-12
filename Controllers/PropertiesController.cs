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

        [Authorize(Roles = "Admin,Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var property = new Property
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    Location = model.Location,
                    CreatedAt = DateTime.Now,
                    SellerId = currentUser.Id
                };

                // معالجة رفع الصور المتعددة
                if (model.ImageFiles != null && model.ImageFiles.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    // إنشاء مجلد uploads إذا لم يكن موجوداً
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in model.ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            // صياغة اسم فريد لكل صورة لتجنب التكرار
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            property.Images.Add(new PropertyImage
                            {
                                ImagePath = "/uploads/" + uniqueFileName
                            });
                        }
                    }
                }

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}