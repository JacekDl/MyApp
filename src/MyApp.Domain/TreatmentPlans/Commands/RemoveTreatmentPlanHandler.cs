using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class RemoveTreatmentPlanCommand(string TreatmentPlanNumber) : IRequest<RemoveTreatmentPlanResult>;

    public record class RemoveTreatmentPlanResult : Result;

    public class RemoveTreatmentPlanHandler : IRequestHandler<RemoveTreatmentPlanCommand, RemoveTreatmentPlanResult>
    {
        private readonly ApplicationDbContext _db;

        public RemoveTreatmentPlanHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RemoveTreatmentPlanResult> Handle(RemoveTreatmentPlanCommand request, CancellationToken ct)
        {
            var plan = await _db.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.Number == request.TreatmentPlanNumber, ct);

            if (plan is null)
            {
                return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
            }

            _db.TreatmentPlans.Remove(plan);

            await _db.SaveChangesAsync(ct);

            return new();
        }
    }
}
