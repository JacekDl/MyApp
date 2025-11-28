using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class LoginViewModel : ViewModelBase
{
    [Required(ErrorMessage = "Pole nie może być puste.")]
    [EmailAddress(ErrorMessage = "Podaj prawidłowy adres email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wprowadź hasło.")] 
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name="Remember me")]
    public bool RememberMe { get; set; }
}
