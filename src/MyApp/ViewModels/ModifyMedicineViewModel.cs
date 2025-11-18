using MyApp.Domain.Medicines;

namespace MyApp.Web.ViewModels;

public class ModifyMedicineViewModel : ViewModelBase
{
    public MedicineDto Medicine { get; set; } = null!;
}
