using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Reviews.Commands;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{

    public record class AddMedicineCommand(string Code, string Name) : IRequest<AddMedicineResult>;

    public record class AddMedicineResult : HResult 
    {
    }

    public class AddMedicineHandler(ApplicationDbContext db) : IRequestHandler<AddMedicineCommand, AddMedicineResult>
    {
        public async Task<AddMedicineResult> Handle(AddMedicineCommand request, CancellationToken ct)
        {
            (var code, var text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Name);

            var exists = await db.Set<Medicine>()
                .AnyAsync(m => m.Code == code, ct);

            if (exists)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            db.Add(new Medicine { Code = code, Name = text });
            await db.SaveChangesAsync(ct);

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

