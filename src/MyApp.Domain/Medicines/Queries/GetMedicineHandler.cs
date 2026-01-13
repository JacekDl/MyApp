using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;


namespace MyApp.Domain.Medicines.Queries    
{
    public record class GetMedicineQuery(int Id) : IRequest<GetMedicineResult>;

    public record class GetMedicineResult : Result<MedicineDto>;

    public class GetMedicineHandler : IRequestHandler<GetMedicineQuery, GetMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public GetMedicineHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetMedicineResult> Handle(GetMedicineQuery request, CancellationToken ct)
        {
            var validator = new GetMedicineValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var result = await _db.Set<Medicine>()
                .Where(m => m.Id == request.Id)
                .Select(m => new MedicineDto(m.Id, m.Code, m.Name))
                .FirstOrDefaultAsync(ct);

            if (result is null)
            {
                return new() { ErrorMessage = "Nie znaleziono leku o podanym Id." };
            }
            return new() { Value = result };
        }

    }

    public class GetMedicineValidator : AbstractValidator<GetMedicineQuery>
    {
        public GetMedicineValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                    .WithMessage("Id leku musi być liczbą dodatnią.");
        }
    }
}
