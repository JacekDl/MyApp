using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class PublicReviewEditViewModel : ViewModelBase
{
    public string Number { get; set; } = string.Empty;
    
    public string Advice { get; set; } = string.Empty;

    [Required(ErrorMessage="Treść opinii nie może być pusta.")]
    public string ReviewText { get; set; } = string.Empty;
}
