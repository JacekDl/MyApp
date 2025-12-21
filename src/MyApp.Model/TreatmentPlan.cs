using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model;

public class TreatmentPlan
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateCompleted { get; set; }

    public string? IdPharmacist { get; set; }
    public User? Pharmacist { get; set; }

    public string? IdPatient { get; set; }
    public User? Patient { get; set; }

    public List<TreatmentPlanMedicine> Medicines { get; set; } = new();

    public TreatmentPlanAdvice? Advice { get; set; }
}
