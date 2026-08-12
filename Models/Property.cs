using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان العقار مطلوب")]
        public string Title { get; set; }

        [Required(ErrorMessage = "الوصف مطلوب")]
        public string Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "الموقع مطلوب")]
        public string Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // قائمة الصور المتعددة
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();

        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }
    }
}