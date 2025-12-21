using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model;

public class TreatmentPlanAdvice
{
    public int Id { get; set; }
    public int IdTreatmentPlan { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = default!;
    public string? AdviceText { get; set; }
}