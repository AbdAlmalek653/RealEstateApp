using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور على الأقل {2} حروف.", MinimumLength = 6)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "يرجى تحديد نوع الحساب")]
        [Display(Name = "نوع الحساب")]
        public string UserType { get; set; } // "Buyer" أو "Seller"
    }
}