using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class ToggleMedicineTakenCommand(
        string IdPatient,
        int TreatmentPlanMedicineId,
        DateTime Date,
        bool IsTaken) : IRequest<ToggleMedicineTakenResult>;

    public record class ToggleMedicineTakenResult : Result;

    public class ToggleMedicineTakenHandler : IRequestHandler<ToggleMedicineTakenCommand, ToggleMedicineTakenResult>
    {
        private readonly ApplicationDbContext _db;

        public ToggleMedicineTakenHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ToggleMedicineTakenResult> Handle(ToggleMedicineTakenCommand request, CancellationToken ct)
        {
            var day = request.Date.Date;
            if (day > DateTime.Today)
            {
                return new() { ErrorMessage = "Nie można potwierdzić przyjęcia leku w przyszłości." };
            }
            var nextDay = day.AddDays(1);

            var belongsToPatient = await (
                from m in _db.Set<TreatmentPlanMedicine>()
                join tp in _db.Set<TreatmentPlan>() on m.IdTreatmentPlan equals tp.Id
                where m.Id == request.TreatmentPlanMedicineId
                where tp.IdPatient == request.IdPatient
                select 1
                ).AnyAsync(ct);

            if (!belongsToPatient)
            {
                return new() { ErrorMessage = "Brak dostępu do tego leku." };
            }

            var existing = await _db.Set<MedicineTakenConfirmation>()
                .FirstOrDefaultAsync(x =>
                    x.IdTreatmentPlanMedicine == request.TreatmentPlanMedicineId &&
                    x.DateTimeTaken >= day &&
                    x.DateTimeTaken < nextDay,
                    ct);

            if (request.IsTaken)
            {
                if (existing is null)
                {
                    _db.Add(new MedicineTakenConfirmation
                    {
                        IdTreatmentPlanMedicine = request.TreatmentPlanMedicineId,
                        DateTimeTaken = request.Date
                    });
                    await _db.SaveChangesAsync();
                }

                return new();
            }

            if (existing is not null)
            {
                _db.Remove(existing);
                await _db.SaveChangesAsync(); 
            }

            return new();
                
        }
    }
}
