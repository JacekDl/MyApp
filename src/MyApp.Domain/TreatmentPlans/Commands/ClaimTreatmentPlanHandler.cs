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
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        if (plan.Claimed)
        {
            return new() { ErrorMessage = "Zalecenia zostały już pobrane." };
        }

        if (plan.DateCreated.AddDays(30) < DateTime.UtcNow)
        {
            return new() { ErrorMessage = "Upłynął termin ważności zaleceń." };
        }

        if (plan.IdPatient is not null && plan.IdPatient != request.PatientId)
        {
            return new() { ErrorMessage = "Zalecenia zostały już użyte." };
        }

        plan.IdPatient = request.PatientId;
        plan.Claimed = true;
        await _db.SaveChangesAsync(ct);
        return new();
    }
}

public class ClaimTreatmentPlanValidator : AbstractValidator<ClaimTreatmentPlanCommand>
{
    private const int ReviewNumberLen = 16;

    public ClaimTreatmentPlanValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Numer jest wymagany.")
            .Length(ReviewNumberLen).WithMessage($"Numer musi mieć {ReviewNumberLen} znaków.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Numer musi się składać z liter i cyfr.");

        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Id użytkownika jest wymagane.");
    }
}
