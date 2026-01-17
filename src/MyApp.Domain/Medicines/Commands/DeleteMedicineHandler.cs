using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public record class DeleteMedicineCommand(int Id) : IRequest<DeleteMedicineResult>;

    public record class DeleteMedicineResult : Result;

    public class DeleteMedicineHandler : IRequestHandler<DeleteMedicineCommand, DeleteMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public DeleteMedicineHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteMedicineResult> Handle(DeleteMedicineCommand request, CancellationToken ct)
        {
            var validator = new DeleteMedicineValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var entity = await _db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
            {
                return new() { ErrorMessage = "Nie znaleziono leku." };
            }

            _db.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class DeleteMedicineValidator : AbstractValidator<DeleteMedicineCommand>
    {
        public DeleteMedicineValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                    .WithMessage("Id leku musi być liczbą dodatnią.");
        }
    }
}
