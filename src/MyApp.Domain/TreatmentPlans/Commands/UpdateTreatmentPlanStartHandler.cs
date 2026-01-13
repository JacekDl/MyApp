
using FluentValidation;
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
            var validator = new UpdateTreatmentPlanStartValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

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

    public class UpdateTreatmentPlanStartValidator
        : AbstractValidator<UpdateTreatmentPlanStartCommand>
    {
        public UpdateTreatmentPlanStartValidator()
        {
            RuleFor(x => x.Number)
                .Must(n => !string.IsNullOrWhiteSpace(n))
                    .WithMessage("Numer planu leczenia nie może być pusty.")
                .Length(16)
                    .WithMessage("Numer planu leczenia musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer planu leczenia może zawierać tylko litery i cyfry.");

            RuleFor(x => x.IdPatient)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id pacjenta nie może być puste.");

            RuleFor(x => x.DateStarted)
                .NotEmpty()
                    .WithMessage("Data rozpoczęcia jest wymagana.")
                .Must(d => d.Date >= DateTime.UtcNow.Date)
                    .WithMessage("Data rozpoczęcia nie może być wcześniejsza niż dzisiaj.");

            RuleFor(x => x.DateCompleted)
                .NotEmpty()
                    .WithMessage("Data zakończenia jest wymagana.")
                .Must((cmd, completed) =>
                    completed.Date > cmd.DateStarted.Date)
                    .WithMessage("Data ukończenia musi być późniejsza niż data rozpoczęcia.");
        }
    }
}
