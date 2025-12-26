using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans.Mappers
{
    public static class TreatmentPlanStatusMapper
    {
        public static string ToPolish(TreatmentPlanStatus status)
            => status switch
            {
                TreatmentPlanStatus.Created => "Utworzony",
                TreatmentPlanStatus.Claimed => "Pobrany",
                TreatmentPlanStatus.Started => "Rozpoczęty",
                TreatmentPlanStatus.Completed => "Zakończony",
                TreatmentPlanStatus.Expired => "Niewykorzystany",
                _ => "Nieznany"
            };
    }
}
