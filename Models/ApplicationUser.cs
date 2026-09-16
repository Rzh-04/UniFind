using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.Models
{
    // Custom user model extending ASP.NET Core IdentityUser
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Department / Major")]
        public string? Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Student ID")]
        public string? StudentOrStaffId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Item> ReportedItems { get; set; } = new List<Item>();
        public virtual ICollection<Claim> ClaimsSubmitted { get; set; } = new List<Claim>();
    }
}
