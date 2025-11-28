using MyApp.Web.Controllers;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    [Required(ErrorMessage = "Pole nie może być puste.")]
    [EmailAddress(ErrorMessage = "Podaj prawidłowy adres email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wprowadź hasło.")] 
    [DataType(DataType.Password)] 
    [MinLength(8, ErrorMessage = "Hasło musi składać się z co najmniej 8 znaków.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdź hasło.")]
    [DataType(DataType.Password)] 
    [Compare(nameof(Password), ErrorMessage = "Podane hasła są różne.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string PostAction { get; set; } = nameof(AccountController.Register);
}
