using MyApp.Model.enums;

namespace MyApp.Model;

public class TreatmentPlan
{
    public int Id { get; set; }
    public string Number { get; set; } = default!;
    public DateTime DateCreated { get; set; }
    public DateTime? DateStarted { get; set; }
    public DateTime? DateCompleted { get; set; }

    public TreatmentPlanStatus Status { get; set; }

    public string? IdPharmacist { get; set; }
    public User? Pharmacist { get; set; }

    public string? IdPatient { get; set; }
    public User? Patient { get; set; }

    public string AdviceFullText { get; set; } = string.Empty;

    public List<TreatmentPlanMedicine> Medicines { get; set; } = new();

    public TreatmentPlanAdvice? Advice { get; set; }

    public TreatmentPlanReview? Review { get; set; }
}
