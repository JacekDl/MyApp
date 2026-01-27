using MyApp.Model;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class ClaimPlanViewModel : ViewModelBase
{
    [Required, MaxLength(TreatmentPlan.NumberLength)]
    public string Number { get; set; } = string.Empty;
}
