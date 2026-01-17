using MyApp.Model.enums;

namespace MyApp.Model;

public class TreatmentPlanMedicine
{
    public int Id { get; set; }

    public int IdTreatmentPlan { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = default!;

    public string MedicineName { get; set; } = default!;
    public string Dosage { get; set; } = default!;
    public TimeOfDay TimeOfDay { get; set; } = default!;

    #region Constants
    public const int MedicineNameMaxLength = 100;
    public const int DosageMaxLength = 100;
    #endregion

}
