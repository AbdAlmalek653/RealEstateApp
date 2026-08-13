using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.ViewModels
{
    public class CreatePropertyViewModel
    {
        [Required(ErrorMessage = "عنوان العقار مطلوب")]
        [Display(Name = "عنوان العقار")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [Display(Name = "المحافظة")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        [Display(Name = "المدينة / المنطقة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(1, 100000, ErrorMessage = "يرجى إدخال مساحة صالحة")]
        [Display(Name = "المساحة (م²)")]
        public double Area { get; set; }

        [Required(ErrorMessage = "عدد الغرف مطلوب")]
        [Range(1, 100, ErrorMessage = "يرجى إدخال عدد غرف صحيح")]
        [Display(Name = "عدد الغرف")]
        public int RoomsCount { get; set; }

        [Required(ErrorMessage = "نوع الفراغة مطلوب")]
        [Display(Name = "نوع الفراغة")]
        public string LegalStatus { get; set; } = "طابو أخضر";

        [Required(ErrorMessage = "نداء الدولة مطلوب")]
        [Display(Name = "نداء الدولة")]
        public string PhoneDialCode { get; set; } = "+963";

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم التواصل مع صاحب العقار")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب")]
        [Display(Name = "الوصف التفصيلي")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0.01, 999999999, ErrorMessage = "يرجى إدخال سعر صالح")]
        [Display(Name = "السعر المطلوب ($)")]
        public decimal Price { get; set; }

        [Display(Name = "صور العقار")]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}