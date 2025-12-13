using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels
{
    public class ResetPasswordRequestViewModel : ViewModelBase
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny adres email.")]
        public string Email { get; set; } = string.Empty;
    }
}
