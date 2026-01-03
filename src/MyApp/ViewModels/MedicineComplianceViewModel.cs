namespace MyApp.Web.ViewModels
{
    public class MedicineComplianceViewModel
    {
        public int TreatmentPlanMedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
    }
}
