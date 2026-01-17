using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record class UpdateInstructionCommand(int Id, string Code, string Text) : IRequest<UpdateInstructionResult>;

    public record class UpdateInstructionResult : Result;

    public class UpdateInstructionHandler : IRequestHandler<UpdateInstructionCommand, UpdateInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public UpdateInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<UpdateInstructionResult> Handle(UpdateInstructionCommand request, CancellationToken ct)
        {
            var validator = new UpdateInstructionValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var entity = await _db.Set<Instruction>()
                .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

            if (entity is null)
            {
                return new() { ErrorMessage = "Nie znaleziono dawkowania." };
            }

            var (code, text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Text);

            var exists = await _db.Set<Instruction>()
                .FirstOrDefaultAsync(m => m.Code == code, ct);

            if (exists != null && exists.Id != request.Id)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            entity.Code = code;
            entity.Text = text;

            var result = await _db.SaveChangesAsync(ct);
            if (result == 0)
            {
                return new() { ErrorMessage = "Nie udało się zaktualizować dawkowania." };
            }
            return new();
        }
    }

    public class UpdateInstructionValidator : AbstractValidator<UpdateInstructionCommand>
    {
        public UpdateInstructionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id dawkowania musi być dodatnie.");

            RuleFor(x => x.Code)
                .Must(code => !string.IsNullOrWhiteSpace(code))
                    .WithMessage("Kod instrukcji nie może być pusty.")
                .MaximumLength(Instruction.CodeMaxLength)
                    .WithMessage($"Kod instrukcji nie może być dłuższy niż {Instruction.CodeMaxLength} znaki.");

            RuleFor(x => x.Text)
                .Must(text => !string.IsNullOrWhiteSpace(text))
                    .WithMessage("Treść instrukcji nie może być pusta.")
                .MaximumLength(Instruction.TextMaxLength)
                    .WithMessage($"Treść instrukcji nie może być dłuższa niż {Instruction.TextMaxLength} znaków.");
        }
    }
}
