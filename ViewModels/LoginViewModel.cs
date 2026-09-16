using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "University Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me?")]
        public bool RememberMe { get; set; }
    }
}
