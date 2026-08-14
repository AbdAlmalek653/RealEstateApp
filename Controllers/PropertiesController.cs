using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .Include(p => p.AssignedAdmin)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        [Authorize(Roles = "SuperAdmin,Admin,Seller")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // جلب وتجهيز قائمة المدراء حصراً إذا كان المستخدم الحالي SuperAdmin
            if (User.IsInRole("SuperAdmin"))
            {
                await PopulateAdminsDropDownListAsync();
            }

            return View();
        }

        [Authorize(Roles = "SuperAdmin,Admin,Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePropertyViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // إزالة كائنات الربط وحقول الأمان من التحقق حتى لا تفشل العملية سراً
            ModelState.Remove("Seller");
            ModelState.Remove("AssignedAdmin");
            ModelState.Remove("Images");
            ModelState.Remove("AssignedAdminId");

            if (ModelState.IsValid)
            {
                string? assignedAdminId = null;

                // السماح باختيار الأدمن فقط إذا كان الحساب سوبر أدمن
                if (User.IsInRole("SuperAdmin") && !string.IsNullOrWhiteSpace(model.AssignedAdminId))
                {
                    var adminExists = await _userManager.FindByIdAsync(model.AssignedAdminId);
                    if (adminExists != null)
                    {
                        assignedAdminId = model.AssignedAdminId;
                    }
                }

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
                    SellerId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Status = PropertyStatus.Available,
                    AssignedAdminId = assignedAdminId
                };

                // حفظ الصور
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in model.ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            property.Images.Add(new PropertyImage { ImagePath = "/uploads/" + uniqueFileName });
                        }
                    }
                }

                _context.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // إذا فشل التحقق، نعيد تجهيز قائمة المشرفين للسوبر أدمن
            if (User.IsInRole("SuperAdmin"))
            {
                await PopulateAdminsDropDownListAsync(model.AssignedAdminId);
            }

            return View(model);
        }

        // GET: Properties/ContactAdmin/5
        public async Task<IActionResult> ContactAdmin(int id, string type = "WhatsApp")
        {
            var property = await _context.Properties
                .Include(p => p.AssignedAdmin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null) return NotFound();

            // 1. تحديد الأدمن المسؤول عن العقار أو جلب أول حساب أدمن متاح كبديل
            var targetAdmin = property.AssignedAdmin;
            if (targetAdmin == null)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                targetAdmin = superAdmins.FirstOrDefault();

                if (targetAdmin == null)
                {
                    var regularAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                    targetAdmin = regularAdmins.FirstOrDefault();
                }
            }

            var targetPhoneNumber = targetAdmin?.PhoneNumber ?? "0500000000";

            // 2. تسجيل الضغطة في جدول السجل (Contact Log)
            var log = new PropertyContactLog
            {
                PropertyId = property.Id,
                AdminId = targetAdmin?.Id,
                ClickedAt = DateTime.UtcNow,
                ContactType = type,
                UserIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.PropertyContactLogs.Add(log);
            await _context.SaveChangesAsync();

            // 3. التوجيه المباشر للتواصل
            var cleanPhone = targetPhoneNumber.Replace("+", "").Replace(" ", "").Trim();
            var messageText = Uri.EscapeDataString($"مرحباً، أرغب بالاستفسار عن العقار: {property.Title} (كود: {property.Id})");

            if (type == "Call")
            {
                return Redirect($"tel:{cleanPhone}");
            }

            return Redirect($"https://wa.me/{cleanPhone}?text={messageText}");
        }

        // دالة مساعدة لتجهيز قائمة المدراء مع الاسم أو الإيميل
        private async Task PopulateAdminsDropDownListAsync(string? selectedAdminId = null)
        {
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var regularAdmins = await _userManager.GetUsersInRoleAsync("Admin");
            var allAdmins = superAdmins.Concat(regularAdmins).DistinctBy(u => u.Id).ToList();

            var adminItems = allAdmins.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = !string.IsNullOrWhiteSpace(a.FullName) ? a.FullName : (a.Email ?? a.UserName ?? "بدون اسم")
            }).ToList();

            ViewBag.AdminsList = new SelectList(adminItems, "Value", "Text", selectedAdminId);
        }
    }
}