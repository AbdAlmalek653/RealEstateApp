using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using RealEstateApp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Controllers
{
    // السماح للأدمن الرئيسي (SuperAdmin) والأدمن المساعد (Admin) بدخول الكنترولر
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

        // ==========================================
        // 1. لوحة التحكم الرئيسية
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.UsersCount = await _userManager.Users.CountAsync();
            ViewBag.PropertiesCount = await _context.Properties.CountAsync();
            return View();
        }

        // ==========================================
        // 2. تعديل بيانات حساب الأدمن الشخصي (Edit Profile)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditAdminProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditAdminProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // تحديث البيانات الأساسية
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // تحديث كلمة المرور في حال تم إدخال كلمة جديدة
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!resetResult.Succeeded)
                {
                    foreach (var error in resetResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "تم تحديث كافة معلومات حسابك بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. إنشاء حساب أدمن مساعد (SuperAdmin حصراً)
        // ==========================================
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

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

        // ==========================================
        // 4. إدارة المستخدمين (SuperAdmin حصراً)
        // ==========================================
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

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

        // ==========================================
        // 5. إدارة العقارات (الأدمن المساعد و SuperAdmin)
        // ==========================================
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

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            // 1. جلب قائمة المدراء وتمريرها للـ View مع تحديد الأدمن الحالي إن وجد
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var regularAdmins = await _userManager.GetUsersInRoleAsync("Admin");
            var allAdmins = superAdmins.Concat(regularAdmins).DistinctBy(u => u.Id).ToList();

            var adminItems = allAdmins.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = !string.IsNullOrWhiteSpace(a.FullName) ? a.FullName : a.Email
            }).ToList();

            ViewBag.AdminsList = new SelectList(adminItems, "Value", "Text", property.AssignedAdminId);

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(Property property)
        {
            // تحويل النص الفارغ إلى null لمنع أخطاء SQLite Foreign Key
            if (string.IsNullOrWhiteSpace(property.AssignedAdminId))
            {
                property.AssignedAdminId = null;
            }

            // إزالة التحقق من كائنات الربط (Navigation Properties) لتفادي أخطاء ModelState غير الضرورية
            ModelState.Remove("Seller");
            ModelState.Remove("AssignedAdmin");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Properties));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Properties.Any(e => e.Id == property.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // 2. إعادة تعبئة القائمة في حال وجود خطأ في التحقق من البيانات
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var regularAdmins = await _userManager.GetUsersInRoleAsync("Admin");
            var allAdmins = superAdmins.Concat(regularAdmins).DistinctBy(u => u.Id).ToList();

            var adminItems = allAdmins.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = !string.IsNullOrWhiteSpace(a.FullName) ? a.FullName : a.Email
            }).ToList();

            ViewBag.AdminsList = new SelectList(adminItems, "Value", "Text", property.AssignedAdminId);

            return View(property);
        }
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
        // GET: Admin/EditUser/{id}
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditAdminProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            ViewBag.TargetUserId = user.Id;
            return View(model);
        }

        // POST: Admin/EditUser/{id}
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, EditAdminProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                }

                TempData["SuccessMessage"] = "تم تحديث بيانات الحساب بنجاح!";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
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
        // GET: Admin/ContactLogs
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ContactLogs()
        {
            var logs = await _context.PropertyContactLogs
                .Include(l => l.Property)
                .Include(l => l.Admin)
                .OrderByDescending(l => l.ClickedAt)
                .ToListAsync();

            return View(logs);
        }
    }
}