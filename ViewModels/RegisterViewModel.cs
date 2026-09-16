using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "University Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Department / Major")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Student / Staff ID Number")]
        [StringLength(50)]
        public string? StudentOrStaffId { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
