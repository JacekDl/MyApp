namespace MyApp.Model;

public class TreatmentPlanAdvice
{
    public int Id { get; set; }
    public int IdTreatmentPlan { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = default!;
    public string? AdviceText { get; set; }

    #region Constants
    public const int AdviceTextMaxLength = 2000;
    #endregion
}