using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

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
               x.IdPharmacist,
               x.IdPatient,
               x.AdviceFullText,
               x.Claimed
           })
           .SingleOrDefaultAsync(ct);

        if (plan is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        if (plan.Claimed)
        {
            return new() { ErrorMessage = "Wykorzystano już kod zaleceń." };
        }

        if (plan.DateCreated.AddDays(30) < DateTime.UtcNow)
        {
            return new () {ErrorMessage = "Minął już termin wykorzystania kodu."};
        }

        var dto = new TreatmentPlanDto(
            plan.Id,
            plan.Number,
            plan.DateCreated,
            plan.IdPharmacist ?? "",
            plan.IdPatient,
            plan.AdviceFullText,
            plan.Claimed
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
