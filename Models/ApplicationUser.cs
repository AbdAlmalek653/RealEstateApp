using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

namespace RealEstateApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        // قائمة العقارات التي يملكها هذا البائع
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}