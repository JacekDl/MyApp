using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class ChangeEmailViewModel
{
    [Required, EmailAddress]
    [Display(Name = "New email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;
}
