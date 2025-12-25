using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class ClaimPlanViewModel : ViewModelBase
{
    [Required, MaxLength(128)]
    public string Number { get; set; } = string.Empty;
}
