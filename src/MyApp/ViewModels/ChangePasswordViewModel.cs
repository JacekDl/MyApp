using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class ChangePasswordViewModel : ViewModelBase
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "Hasło musi składać się z co najmniej 8 znaków.")]
    [Required(ErrorMessage = "Pole nie może być puste.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pole nie może być puste.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Podane hasła są różne.")]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
