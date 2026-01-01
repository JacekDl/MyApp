namespace MyApp.Domain.TreatmentPlans;

public record TreatmentPlanDto
(
    int Id,
    string Number,
    DateTime DateCreated,
    DateTime? DateStarted,
    DateTime? DateCompleted,
    string? IdPharmacist,
    string? IdPatient,
    string AdviceFullText,
    string Status,
    List<TreatmentPlanReviewEntryDto> ReviewEntries
);
