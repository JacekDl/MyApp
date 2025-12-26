using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.TreatmentPlans.Mappers;

namespace MyApp.Domain.TreatmentPlans.Queries;

public record class GetTreatmentPlanQuery(string Number) : IRequest<GetTreatmentPlanResult>;

public record class GetTreatmentPlanResult : Result<TreatmentPlanDto>;

public class GetTreatmentPlanHandler : IRequestHandler<GetTreatmentPlanQuery, GetTreatmentPlanResult>
{

    private readonly ApplicationDbContext _db;

    public GetTreatmentPlanHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<GetTreatmentPlanResult> Handle(GetTreatmentPlanQuery request, CancellationToken ct)
    {
        var validator = new GetTreatmentPlanValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var plan = await _db.TreatmentPlans
           .Where(r => r.Number == request.Number)
           .Select(x => new
           {
               x.Id,
               x.Number,
               x.DateCreated,
               x.DateStarted,
               x.DateCompleted,
               x.IdPharmacist,
               x.IdPatient,
               x.AdviceFullText,
               x.Status
           })
           .SingleOrDefaultAsync(ct);

        if (plan is null)
        {
            return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
        }

        var dto = new TreatmentPlanDto(
            plan.Id,
            plan.Number,
            plan.DateCreated,
            plan.DateStarted,
            plan.DateCompleted,
            plan.IdPharmacist ?? "",
            plan.IdPatient ?? "",
            plan.AdviceFullText,
            TreatmentPlanStatusMapper.ToPolish(plan.Status)
            );

        return new() { Value = dto  };
    }

    public class GetTreatmentPlanValidator : AbstractValidator<GetTreatmentPlanQuery>
    {
        public GetTreatmentPlanValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                    .WithMessage("Numer tokenu jest wymagany.")
                .Length(16)
                    .WithMessage("Numer tokenu musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer tokenu może zawierać tylko litery i cyfry.");
        }
    }
}
