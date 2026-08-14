namespace RealEstateApp.Models
{
    public class PropertyContactLog
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        // الأدمن المستهدف بالتواصل
        public string? AdminId { get; set; }
        public ApplicationUser? Admin { get; set; }

        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
        public string? ContactType { get; set; } // WhatsApp or Call
        public string? UserIpAddress { get; set; }
    }
}