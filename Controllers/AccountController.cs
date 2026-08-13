using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Models;
using RealEstateApp.ViewModels;
using System.Threading.Tasks;

namespace RealEstateApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. صفحة إنشاء حساب (عرض)
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // 1. صفحة إنشاء حساب (معالجة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber // تخزين رقم الهاتف من مودل التسجيل
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // إعطاء دور البائع/المشتري تلقائياً لكافة الحسابات المنشأة جديداً
                    await _userManager.AddToRoleAsync(user, "Seller");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // 2. صفحة تسجيل الدخول (عرض)
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // 2. صفحة تسجيل الدخول (معالجة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة، يرجى التحقق من البريد أو كلمة المرور.");
            }

            return View(model);
        }

        // 3. تسجيل الخروج
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // 4. صفحة رفض الوصول
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}