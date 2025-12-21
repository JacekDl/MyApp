using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class TreatmentPlanCreateViewModel : ViewModelBase
{
    public List<MedicineViewModel> Medicines { get; set; } = [];

    public List<AdviceViewModel> Advices { get; set; } = [];
}
