using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityLostAndFound.Models
{
    // Represents a reported lost or found item post
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Item Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Detailed Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Item Type")]
        public ItemType ItemType { get; set; } = ItemType.Lost;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required]
        [Display(Name = "Campus Location")]
        public int LocationId { get; set; }
        public virtual Location? Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Lost / Found")]
        public DateTime DateLostOrFound { get; set; } = DateTime.Today;

        [Display(Name = "Photo")]
        public string? ImagePath { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.Reported;

        [StringLength(50)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Reward / Notes")]
        public string? RewardDetails { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Claims submitted for this item
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
