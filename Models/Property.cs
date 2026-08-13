using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Models
{
    public enum PropertyStatus
    {
        [Display(Name = "متاح")]
        Available = 0,

        [Display(Name = "تم البيع")]
        Sold = 1
    }

    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان العقار مطلوب")]
        [StringLength(150, ErrorMessage = "يجب ألا يتجاوز العنوان 150 حرفاً")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(1, 100000, ErrorMessage = "يرجى إدخال مساحة صالحة")]
        public double Area { get; set; } // المساحة بالمتر المربع

        [Required(ErrorMessage = "عدد الغرف مطلوب")]
        [Range(1, 100, ErrorMessage = "يرجى إدخال عدد غرف صحيح")]
        public int RoomsCount { get; set; }

        [Required(ErrorMessage = "نوع الفراغة مطلوب")]
        public string LegalStatus { get; set; } = "طابو أخضر"; // طابو أخضر / مشاع بعقد تنازل / بعقد تنازل

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "نداء الدولة مطلوب")]
        public string PhoneDialCode { get; set; } = "+963";

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "يرجى إدخال رقم هاتف صحيح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0.01, 999999999, ErrorMessage = "يرجى إدخال سعر صالح")]
        [Column(TypeName = "decimal(18,2)")] // لتفادي تحذيرات EF Core الخاطئة الخاصة بالـ decimal
        public decimal Price { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Available;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- علاقة البائع (Seller / ApplicationUser) ---
        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }

        // --- قائمة الصور المتعددة ---
        public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}