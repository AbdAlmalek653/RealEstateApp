namespace RealEstateApp.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } // مسار الصورة المسجل في المجلد المحلي

        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}