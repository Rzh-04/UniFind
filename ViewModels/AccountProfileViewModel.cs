using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.ViewModels
{
    public class AccountProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "New password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new passwords do not match.")]
        [Display(Name = "Confirm new password")]
        public string? ConfirmNewPassword { get; set; }
    }
}
