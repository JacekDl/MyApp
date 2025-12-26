
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans.Commands
{

    public record class UpdateTreatmentPlanStartCommand(
        string Number, 
        string IdPatient,
        DateTime DateStarted,
        DateTime DateCompleted) : IRequest<UpdateTreatmentPlanStartResult>;

    public record class UpdateTreatmentPlanStartResult : Result;
    public class UpdateTreatmentPlanStartHandler : IRequestHandler<UpdateTreatmentPlanStartCommand, UpdateTreatmentPlanStartResult>
    {
        private readonly ApplicationDbContext _db;

        public UpdateTreatmentPlanStartHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<UpdateTreatmentPlanStartResult> Handle(UpdateTreatmentPlanStartCommand request, CancellationToken ct)
        {
            var plan = await _db.TreatmentPlans
            .FirstOrDefaultAsync(x => x.Number == request.Number, ct);

            if (plan is null)
            {
                return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
            }

            if (!string.Equals(plan.IdPatient, request.IdPatient, StringComparison.OrdinalIgnoreCase))
            {
                return new() { ErrorMessage = "Brak uprawnień do edycji tego planu." };
            }

            var today = DateTime.UtcNow.Date;
            if (request.DateStarted.Date < today)
            {
                return new() { ErrorMessage = "Data rozpoczęcia nie może być wcześniejsza niż dzisiaj." };
            }

            if (request.DateCompleted.Date <= request.DateStarted.Date)
            {
                return new() { ErrorMessage = "Data ukończenia musi być późniejsza niż data rozpoczęcia." };
            }

            if (plan.Status is TreatmentPlanStatus.Completed or TreatmentPlanStatus.Expired)
            {
                return new() { ErrorMessage = "Nie można zmienić dat dla zakończonego lub wygasłego planu." };
            }

            plan.DateStarted = request.DateStarted.Date;
            plan.DateCompleted = request.DateCompleted.Date;

            plan.Status = TreatmentPlanStatus.Started;

            await _db.SaveChangesAsync(ct);

            return new();
        }
    }
}
