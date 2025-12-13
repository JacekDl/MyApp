using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class ResetPasswordConfirmViewModel : ViewModelBase
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są takie same.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}