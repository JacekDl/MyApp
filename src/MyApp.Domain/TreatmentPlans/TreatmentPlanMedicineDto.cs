using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans
{
    public record TreatmentPlanMedicineDto
    (
        int IdTreatmentPlan,
        string TreatmentPlanNumber,
        string MedicineName,
        string Dosage,
        TimeOfDay TimeOfDay
    );
}
