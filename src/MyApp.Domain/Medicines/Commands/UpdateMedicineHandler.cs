using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public record class UpdateMedicineCommand(int Id, string Code, string Name) : IRequest<UpdateMedicineResult>;

    public record class UpdateMedicineResult : Result;

    public class UpdateMedicineHandler : IRequestHandler<UpdateMedicineCommand, UpdateMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public UpdateMedicineHandler( ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<UpdateMedicineResult> Handle(UpdateMedicineCommand request, CancellationToken ct)
        {
            var validator = new UpdateMedicineValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join("; ", validator.Errors.Select(e => e.ErrorMessage)) };
            }
            var entity = await _db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
            {
                return new() { ErrorMessage = "Nie znaleziono leku w bazie." };
            }

            var (code, name) = FormatStringHelper.FormatCodeAndText(request.Code, request.Name);

            var exists = await _db.Set<Medicine>()
                .FirstOrDefaultAsync(m => m.Code == code, ct);

            if (exists != null && exists.Id != request.Id)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            entity.Code = code;
            entity.Name = name;

            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class UpdateMedicineValidator : AbstractValidator<UpdateMedicineCommand>
    {
        public UpdateMedicineValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id leku musi być dodatnie.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod leku jest wymagany.")
                .MaximumLength(32).WithMessage("Kod leku nie może być dłuższy niż 32 znaki.")
                .Must(code => !string.IsNullOrWhiteSpace(code))
                .WithMessage("Kod leku nie może być pusty.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nazwa leku jest wymagana.")
                .MaximumLength(128).WithMessage("Nazwa leku nie może być dłuższa niż 128 znaków.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Nazwa leku nie może być pusta.");
        }
    }
}
