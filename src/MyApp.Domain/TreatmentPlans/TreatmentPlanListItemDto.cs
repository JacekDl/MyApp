using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans;

public record TreatmentPlanListItemDto(
    int Id,
    string Number,
    DateTime DateCreated,
    DateTime? DateStarted,
    DateTime? DateCompleted,
    string AdviceFullText,
    string Status,
    bool UnreadForPatient,
    bool UnreadForPharmacist
);