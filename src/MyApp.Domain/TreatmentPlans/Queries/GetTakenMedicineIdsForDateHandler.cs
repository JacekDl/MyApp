using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.TreatmentPlans.Queries
{

    public record class GetTakenMedicineIdsForDateQuery(string IdPatient, DateTime Date) : IRequest<GetTakenMedicineIdsForDateResult>;

    public record class GetTakenMedicineIdsForDateResult : Result<HashSet<int>>;

    public class GetTakenMedicineIdsForDateHandler : IRequestHandler<GetTakenMedicineIdsForDateQuery, GetTakenMedicineIdsForDateResult>
    {

        private readonly ApplicationDbContext _db;

        public GetTakenMedicineIdsForDateHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetTakenMedicineIdsForDateResult> Handle(GetTakenMedicineIdsForDateQuery request, CancellationToken ct)
        {
            var validator = new GetTakenMedicineIdsForDateValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var day = request.Date.Date;
            var nextDay = day.AddDays(1);

            var ids = await(
                from c in _db.Set<MyApp.Model.MedicineTakenConfirmation>().AsNoTracking()
                join m in _db.Set<MyApp.Model.TreatmentPlanMedicine>().AsNoTracking()
                    on c.IdTreatmentPlanMedicine equals m.Id
                join tp in _db.Set<MyApp.Model.TreatmentPlan>().AsNoTracking()
                    on m.IdTreatmentPlan equals tp.Id
                where tp.IdPatient == request.IdPatient
                where c.DateTimeTaken >= day && c.DateTimeTaken < nextDay
                select c.IdTreatmentPlanMedicine
            ).Distinct().ToListAsync(ct);

            return new() { Value = ids.ToHashSet() };
        }
    }

    public class GetTakenMedicineIdsForDateValidator : AbstractValidator<GetTakenMedicineIdsForDateQuery>
    {
        public GetTakenMedicineIdsForDateValidator()
        {
            RuleFor(x => x.IdPatient)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id pacjenta nie może być puste.");

            RuleFor(x => x.Date)
                .NotEmpty()
                    .WithMessage("Data jest wymagana.");
        }
    }
}
