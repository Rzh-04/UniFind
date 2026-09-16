using System.ComponentModel.DataAnnotations;
using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.ViewModels
{
    public class ClaimCreateViewModel
    {
        [Required]
        public int ItemId { get; set; }
        public ItemType ItemType { get; set; }

        public string ItemTitle { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public string? ItemImagePath { get; set; }
        public string? ItemLocationName { get; set; }
        public string? ItemRewardDetails { get; set; }

        [StringLength(300)]
        [Display(Name = "Collection location")]
        public string? CollectionLocation { get; set; }

        [Required(ErrorMessage = "Please provide detailed proof of ownership.")]
        [StringLength(2000, ErrorMessage = "Proof description cannot exceed 2000 characters.")]
        [Display(Name = "Proof and distinguishing details")]
        public string ProofOfOwnership { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your contact phone number.")]
        [StringLength(50)]
        [Display(Name = "Contact Phone Number")]
        public string ContactPhoneNumber { get; set; } = string.Empty;
    }
}
