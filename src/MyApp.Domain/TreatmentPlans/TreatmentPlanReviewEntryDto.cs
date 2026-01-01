using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans
{
    public record TreatmentPlanReviewEntryDto
    (
        int Id,
        DateTime DateCreated,
        ConversationParty Author,
        string Text
        );
}
