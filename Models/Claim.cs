using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityLostAndFound.Models
{
    // Represents a student claim requesting return of a found item
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [Required]
        public string ClaimerUserId { get; set; } = string.Empty;

        [ForeignKey("ClaimerUserId")]
        public virtual ApplicationUser? ClaimerUser { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Proof of Ownership")]
        public string ProofOfOwnership { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Contact Phone Number")]
        public string ContactPhoneNumber { get; set; } = string.Empty;

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Staff / Finder Review Notes")]
        public string? AdminOrFinderNotes { get; set; }

        public DateTime ClaimDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedDate { get; set; }
    }
}
