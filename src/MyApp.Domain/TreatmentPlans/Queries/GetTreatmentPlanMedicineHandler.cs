
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.TreatmentPlans.Queries
{

    public record class GetTreatmentPlanMedicinesQuery(string PatientId, DateTime Date) : IRequest<GetTreatmentPlanMedicinesResult>;

    public record class GetTreatmentPlanMedicinesResult : Result<List<TreatmentPlanMedicineDto>>;
    public class GetTreatmentPlanMedicinesHandler : IRequestHandler<GetTreatmentPlanMedicinesQuery, GetTreatmentPlanMedicinesResult>
    {
        private readonly ApplicationDbContext _db;

        public GetTreatmentPlanMedicinesHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetTreatmentPlanMedicinesResult> Handle(GetTreatmentPlanMedicinesQuery request, CancellationToken ct)
        {
            var dayStart = request.Date.Date;
            var nextDayStart = dayStart.AddDays(1);

            var query =
                from m in _db.Set<TreatmentPlanMedicine>().AsNoTracking()
                join tp in _db.Set<TreatmentPlan>().AsNoTracking()
                    on m.IdTreatmentPlan equals tp.Id
                where tp.IdPatient == request.PatientId
                where tp.DateStarted.HasValue && tp.DateCompleted.HasValue
                where tp.DateStarted!.Value < nextDayStart
                where tp.DateCompleted!.Value >= dayStart
                orderby tp.Number, (int)m.TimeOfDay, m.MedicineName
                select new TreatmentPlanMedicineDto(
                    m.Id,
                    tp.Id,
                    tp.Number,
                    m.MedicineName,
                    m.Dosage,
                    m.TimeOfDay
                );

            var medicines = await query.ToListAsync(ct);

            return new() { Value = medicines };
        }
    }
}
