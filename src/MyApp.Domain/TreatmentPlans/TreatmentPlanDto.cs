namespace MyApp.Domain.TreatmentPlans;

public record TreatmentPlanDto
(
    int Id,
    string Number,
    DateTime DateCreated,
    string? IdPharmacist,
    string? IdPatient,
    string AdviceFullText,
    bool Claimed
);
