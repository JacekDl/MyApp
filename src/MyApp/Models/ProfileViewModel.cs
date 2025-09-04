using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class ProfileViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "New password")]
        public string? NewPassword { get; set; }


        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [Display(Name = "Confirm new password")]
        public string? ConfirmNewPassword { get; set; }

        [MaxLength(16)]
        public string? Name { get; set; }

        [MaxLength(32)]
        public string? PharmacyName { get; set; }

        [MaxLength(32)]
        public string? PharmacyCity { get; set; }
    }
}
