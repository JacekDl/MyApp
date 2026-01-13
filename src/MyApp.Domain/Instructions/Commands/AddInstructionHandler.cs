using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record class AddInstructionCommand(string Code, string Text) : IRequest<AddInstructionResult>;

    public record class AddInstructionResult : Result;

    public class AddInstructionHandler : IRequestHandler<AddInstructionCommand, AddInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public AddInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AddInstructionResult> Handle(AddInstructionCommand request, CancellationToken ct)
        {
            var validator = new AddInstructionValidator().Validate(request);

            if(!validator.IsValid)
            {
                var errors = string.Join(";", validator.Errors.Select(e => e.ErrorMessage));
                return new() { ErrorMessage = errors };
            }

            var (code, text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Text);

            var exists = await _db.Set<Instruction>()
                .AnyAsync(i => i.Code == code, ct);

            if (exists)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            _db.Add(new Instruction { Code = code, Text = text });
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class AddInstructionValidator : AbstractValidator<AddInstructionCommand>
    {
        public AddInstructionValidator()
        {
            RuleFor(x => x.Code)
                .Must(code => !string.IsNullOrWhiteSpace(code))
                    .WithMessage("Kod instrukcji jest wymagany.")
                .MaximumLength(32)
                    .WithMessage("Kod instrukcji nie może być dłuższy niż 32 znaki.");

            RuleFor(x => x.Text)
                .Must(text => !string.IsNullOrWhiteSpace(text))
                    .WithMessage("Treść instrukcji jest wymagana.")
                .MaximumLength(256)
                    .WithMessage("Treść instrukcji nie może być dłuższa niż 256 znaków.");
        }
    }


}
