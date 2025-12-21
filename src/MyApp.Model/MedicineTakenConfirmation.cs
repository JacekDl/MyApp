using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model;

public class MedicineTakenConfirmation
{
    public int Id { get; set; }
    public int IdTreatmentPlanMedicine { get; set; }
    public TreatmentPlanMedicine TreatmentPlanMedicine { get; set; } = default!;
    public DateTime DateTimeTaken { get; set; }
}
