namespace MyApp.Web.ViewModels
{
    public class TreatmentPlanComplianceViewModel : ViewModelBase
    {
        public int TreatmentPlanId { get; set; }

        public string Number {  get; set; }

        public DateTime? DateStarted { get; set; }

        public IReadOnlyList<MedicineComplianceViewModel> Medicines { get; set; }
            = Array.Empty<MedicineComplianceViewModel>();
    }
}
