using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Domain.Users;

public record CreateTreatmentPlanMedicineDTO(string MedicineName, string MedicineDosage, string MedicineFrequency);
