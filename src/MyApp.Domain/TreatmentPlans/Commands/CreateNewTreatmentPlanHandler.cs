using MediatR;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Users;
using MyApp.Model;
using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class CreateTreatmentPlanCommand(
        string PharmacistId,
        IReadOnlyList<CreateTreatmentPlanMedicineDTO> Medicines,
        string? Advice) : IRequest<CreateTreatmentPlanResult>;

    public record class CreateTreatmentPlanResult : Result;

    public class CreateNewTreatmentPlanHandler : IRequestHandler<CreateTreatmentPlanCommand, CreateTreatmentPlanResult>
    {
        private readonly ApplicationDbContext _db;

        public CreateNewTreatmentPlanHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreateTreatmentPlanResult> Handle(CreateTreatmentPlanCommand request, CancellationToken ct)
        {
            var plan = new TreatmentPlan
            {
                DateCreated = DateTime.UtcNow,
                DateCompleted = null,
                IdPharmacist = request.PharmacistId
            };

            _db.TreatmentPlans.Add(plan);
            await _db.SaveChangesAsync(ct);

            var medicinesToAdd = new List<TreatmentPlanMedicine>();

            foreach(var med in request.Medicines)
            {
                var times = ExpandFrequency(med.MedicineFrequency);

                foreach(var time in times)
                {
                    medicinesToAdd.Add(new TreatmentPlanMedicine
                    {
                        IdTreatmentPlan = plan.Id,
                        MedicineName = med.MedicineName,
                        Dosage = med.MedicineDosage,
                        TimeOfDay = time
                    });
                }
            }

            if (medicinesToAdd.Count > 0)
            {
                _db.TreatmentPlanMedicines.AddRange(medicinesToAdd);
            }

            if (!string.IsNullOrWhiteSpace(request.Advice))
            {
                var advice = new TreatmentPlanAdvice
                {
                    IdTreatmentPlan = plan.Id,
                    AdviceText = request.Advice.Trim()
                };
                _db.TreatmentPlanAdvices.Add(advice);
            }

            await _db.SaveChangesAsync(ct);
            return new();
        }

        private static IReadOnlyList<TimeOfDay> ExpandFrequency(string token) =>
        token switch
        {
            "raz_dziennie_rano" => new[] { TimeOfDay.Rano },
            "raz_dziennie_wieczorem" => new[] { TimeOfDay.Wieczor },
            "dwa_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Wieczor },
            "trzy_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Poludnie, TimeOfDay.Wieczor },
            "cztery_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Poludnie, TimeOfDay.Popoludnie, TimeOfDay.Wieczor },
            _ => Array.Empty<TimeOfDay>()
        };
    }
}
