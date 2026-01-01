namespace MyApp.Model
{
    public class TreatmentPlanReview
    {
        public int Id { get; set; }

        public int IdTreatmentPlan {  get; set; }
        public TreatmentPlan TreatmentPlan { get; set; } = default!;

        public bool UnreadForPharmacist { get; set; }
        public bool UnreadForPatient { get; set; }

        public List<ReviewEntry> ReviewEntries { get; set; } = new();
    }
}
