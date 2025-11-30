using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{

    public record class AddMedicineCommand(string Code, string Name) : IRequest<AddMedicineResult>;

    public record class AddMedicineResult : Result
    {
    }

    public class AddMedicineHandler : IRequestHandler<AddMedicineCommand, AddMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public AddMedicineHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<AddMedicineResult> Handle(AddMedicineCommand request, CancellationToken ct)
        {
            var validator = new AddMedicineValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            (var code, var text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Name);

            var exists = await _db.Set<Medicine>()
                .AnyAsync(m => m.Code == code, ct);

            if (exists)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            _db.Add(new Medicine { Code = code, Name = text });
            await _db.SaveChangesAsync(ct);

            return new();
        }
    }

    public class AddMedicineValidator : AbstractValidator<AddMedicineCommand>
    {
        public AddMedicineValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod leku jest wymagany.")
                .MaximumLength(32).WithMessage("Kod leku nie może być dłuższy niż 32 znaków.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nazwa leku jest wymagana.")
                .MaximumLength(128).WithMessage("Nazwa leku nie może być dłuższa niż 128 znaków.");
        }
    }
}

