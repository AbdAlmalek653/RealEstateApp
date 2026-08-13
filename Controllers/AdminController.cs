using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Controllers
{
    // السماح للأدمن الرئيسي (SuperAdmin) والآدمن المساعد (Admin) بدخول الكنترولر
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // 1. عرض صفحة إنشاء حساب أدمن جديد (SuperAdmin حصراً)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // 2. معالجة إنشاء الحساب من الـ SuperAdmin
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "جميع الحقول مطلوبة.");
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "هذا البريد الإلكتروني مستخدم بالفعل.");
                return View();
            }

            var newAdmin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newAdmin, password);

            if (result.Succeeded)
            {
                // إعطاء دور Admin المساعد للحساب الجديد
                await _userManager.AddToRoleAsync(newAdmin, "Admin");

                TempData["SuccessMessage"] = "تم إنشاء حساب الأدمن المساعد بنجاح!";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
        // 1. لوحة التحكم الرئيسية الخاصة بالآدمن
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.UsersCount = await _userManager.Users.CountAsync();
            ViewBag.PropertiesCount = await _context.Properties.CountAsync();
            return View();
        }

        // 2. إدارة كافة المستخدمين (متاحة حصراً للـ SuperAdmin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // 3. حذف حساب مستخدم (متاحة حصراً للـ SuperAdmin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        // 4. إدارة كافة العقارات (تعديل وحذف وحالة)
        [HttpGet]
        public async Task<IActionResult> Properties()
        {
            var properties = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }

        // 5. تعديل عقار من قبل الأدمن
        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(Property property)
        {
            if (ModelState.IsValid)
            {
                _context.Update(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Properties));
            }
            return View(property);
        }

        // 6. حذف عقار من قبل الأدمن
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Properties));
        }

        // 7. تغيير حالة العقار (متاح / تم البيع) - متاح لكلا الآدمنين
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                property.Status = property.Status == PropertyStatus.Available
                    ? PropertyStatus.Sold
                    : PropertyStatus.Available;

                _context.Update(property);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Properties));
        }
    }
}