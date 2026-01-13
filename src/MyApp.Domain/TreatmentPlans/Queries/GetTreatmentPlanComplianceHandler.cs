using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.TreatmentPlans.Queries
{
    public record class GetTreatmentPlanComplianceQuery(string Number) : IRequest<GetTreatmentPlanComplianceResult>;

    public record class GetTreatmentPlanComplianceResult : Result<TreatmentPlanComplianceDto>;

    public class GetTreatmentPlanComplianceHandler : IRequestHandler<GetTreatmentPlanComplianceQuery, GetTreatmentPlanComplianceResult>
    {
        private readonly ApplicationDbContext _db;

        public GetTreatmentPlanComplianceHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetTreatmentPlanComplianceResult> Handle(GetTreatmentPlanComplianceQuery request, CancellationToken ct)
        {
            var validator = new GetTreatmentPlanComplianceValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var now = DateTime.UtcNow;

            var plan = await _db.TreatmentPlans
                .AsNoTracking()
                .Include(tp => tp.Medicines)
                .FirstOrDefaultAsync(tp => tp.Number == request.Number, ct);

            if (plan is null)
            {
                return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
            }

            if (plan.DateStarted is null && plan.Status == Model.enums.TreatmentPlanStatus.Completed)
            {
                return new() { ErrorMessage = "Pacjent nie pobrał planu leczenia w celu zapisu przyjmowanych dawek." };
            }

            if (plan.DateStarted is null || plan.DateStarted > now)
            {
                return new() { ErrorMessage = "Plan leczenia nie został jeszcze rozpoczęty." };
            }



            var start = plan.DateStarted.Value;

            var daysInclusive = (now.Date - start.Date).Days + 1;
            if (daysInclusive < 1) daysInclusive = 1;

            var dosesPerDayByName = plan.Medicines
                .GroupBy(m => m.MedicineName)
                .ToDictionary(g => g.Key, g => g.Count());

            var medicineNameById = plan.Medicines
                .ToDictionary(m => m.Id, m => m.MedicineName);

            var medicineIds = medicineNameById.Keys.ToList();

            var confirmationMedicineIds = await _db.Set<MedicineTakenConfirmation>()
                .AsNoTracking()
                .Where(c =>
                    medicineIds.Contains(c.IdTreatmentPlanMedicine) &&
                    c.DateTimeTaken >= start &&
                    c.DateTimeTaken <= now)
                .Select(c => c.IdTreatmentPlanMedicine)
                .ToListAsync(ct);

            var takenCountsByName = confirmationMedicineIds
                .Select(id => medicineNameById[id])
                .GroupBy(name => name)
                .ToDictionary(g => g.Key, g => g.Count());

            var rows = dosesPerDayByName
                .Select(kvp =>
                {
                    var medicineName = kvp.Key;
                    var dosesPerDay = kvp.Value;

                    takenCountsByName.TryGetValue(medicineName, out var taken);

                    var expected = daysInclusive * dosesPerDay;

                    var percentage = expected <= 0
                        ? 0m
                        : Math.Min(100m, Math.Round((decimal)taken * 100m / expected, 2));

                    return new MedicineComplianceDto(
                        TreatmentPlanMedicineId: 0,
                        MedicineName: medicineName,
                        Percentage: percentage
                    );
                })
                .OrderBy(x => x.MedicineName)
                .ToList();

            var dto = new TreatmentPlanComplianceDto(
                TreatmentPlanId: plan.Id,
                Number: plan.Number,
                DateStarted: plan.DateStarted,
                Medicines: rows
            );

            return new() { Value = dto };
        }
    }

    public class GetTreatmentPlanComplianceValidator : AbstractValidator<GetTreatmentPlanComplianceQuery>
    {
        public GetTreatmentPlanComplianceValidator()
        {
            RuleFor(x => x.Number)
                .Must(n => !string.IsNullOrWhiteSpace(n))
                    .WithMessage("Numer planu leczenia nie może być pusty.")
                .Length(16)
                    .WithMessage("Numer planu leczenia musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer planu leczenia może zawierać tylko litery i cyfry.");
        }
    }
}
