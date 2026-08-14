using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.ViewModels
{
    public class EditAdminProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة (اتركها فارغة إذا لم ترد تغييرها)")]
        [StringLength(100, ErrorMessage = "يجب ألا تقل كلمة المرور عن {2} حروف", MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}