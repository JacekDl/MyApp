using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;


public class ReviewCreateViewModel : ViewModelBase
{
    [Required(ErrorMessage = "Treść zalecenia nie może być pusta.")]
    public string Advice { get; set; } = string.Empty;
}
