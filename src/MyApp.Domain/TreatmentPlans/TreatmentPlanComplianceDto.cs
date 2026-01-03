namespace MyApp.Domain.TreatmentPlans
{
    public record TreatmentPlanComplianceDto
    (
        int TreatmentPlanId,
        string Number,
        DateTime? DateStarted,
        IReadOnlyList<MedicineComplianceDto> Medicines
    );

    public record MedicineComplianceDto
    (
        int TreatmentPlanMedicineId,
        string MedicineName,
        decimal Percentage
    );
}
