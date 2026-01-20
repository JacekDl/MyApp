using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class PromoteToPharmacistViewModel : ViewModelBase
{
    [Required(ErrorMessage = "Podaj numer prawa wykonywania zawodu.")]
    [RegularExpression(@"^\d{8}$",
        ErrorMessage = "Numer PWZF musi składać się dokładnie z 8 cyfr.")]
    [Display(Name = "Numer PWZF")]
    public string NumerPWZF { get; set; } = string.Empty;
}