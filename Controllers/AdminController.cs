using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;

namespace RealEstateApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 1. لوحة التحكم الرئيسية الخاصة بالآدمن
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.UsersCount = await _userManager.Users.CountAsync();
            ViewBag.PropertiesCount = await _context.Properties.CountAsync();
            return View();
        }

        // 2. إدارة كافة المستخدمين
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // 3. حذف حساب مستخدم
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

        // 4. إدارة كافة العقارات (تعديل وحذف)
        [HttpGet]
        public async Task<IActionResult> Properties()
        {
            var properties = await _context.Properties
                .Include(p => p.Seller)
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
    }
}