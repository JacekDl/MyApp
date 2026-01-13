using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.TreatmentPlans.Commands;

public record class ClaimTreatmentPlanCommand(string Number, string PatientId) : IRequest<ClaimTreatmentPlanResult>;

public record class ClaimTreatmentPlanResult : Result;

public class ClaimTreatmentPlanHandler : IRequestHandler<ClaimTreatmentPlanCommand, ClaimTreatmentPlanResult>
{
    private readonly ApplicationDbContext _db;

    public ClaimTreatmentPlanHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimTreatmentPlanResult> Handle(ClaimTreatmentPlanCommand request, CancellationToken ct)
    {
        var validator = new ClaimTreatmentPlanValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var plan = await _db.TreatmentPlans.SingleOrDefaultAsync(r => r.Number == request.Number);

        if (plan is null)
        {
            return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
        }

        //plan pobrany i potencjalnie wykorzystany
        if (plan.Status >= Model.enums.TreatmentPlanStatus.Claimed) 
        {
            return new() { ErrorMessage = "Plan lecznia został już pobrany." };
        }

        if (plan.DateCreated.AddDays(30) < DateTime.UtcNow)
        {
            plan.Status = Model.enums.TreatmentPlanStatus.Expired;
            await _db.SaveChangesAsync(ct);
            return new() { ErrorMessage = "Upłynął termin ważności planu lecznia." };
        }

        if (plan.IdPatient is not null && plan.IdPatient != request.PatientId)
        {
            return new() { ErrorMessage = "Plan leczenia został już użyty." };
        }

        plan.IdPatient = request.PatientId;
        plan.Status = Model.enums.TreatmentPlanStatus.Claimed;
        await _db.SaveChangesAsync(ct);
        return new();
    }
}

public class ClaimTreatmentPlanValidator : AbstractValidator<ClaimTreatmentPlanCommand>
{
    public ClaimTreatmentPlanValidator()
    {
        RuleFor(x => x.Number)
            .Must(t => !string.IsNullOrWhiteSpace(t))
                .WithMessage("Numer jest wymagany.")
            .Length(16)
                .WithMessage($"Numer musi mieć dokładnie 16 znaków.")
            .Matches("^[A-Za-z0-9]+$")
                .WithMessage("Numer musi się składać z liter i cyfr.");

        RuleFor(x => x.PatientId)
            .Must(p => !string.IsNullOrWhiteSpace(p))
                .WithMessage("Id użytkownika jest wymagane.");
    }
}
