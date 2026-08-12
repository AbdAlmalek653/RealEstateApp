using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.ViewModels
{
    public class CreatePropertyViewModel
    {
        [Required(ErrorMessage = "عنوان العقار مطلوب")]
        public string Title { get; set; }

        [Required(ErrorMessage = "الوصف مطلوب")]
        public string Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "الموقع مطلوب")]
        public string Location { get; set; }

        // استخدام قائمة من IFormFile لاستقبال ملفات الصور من نموذج HTML
        [Display(Name = "صور العقار (يمكن اختيار أكثر من صورة)")]
        public List<IFormFile> ImageFiles { get; set; }
    }
}