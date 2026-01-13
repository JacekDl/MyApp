using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record class DeleteInstructionCommand(int Id) : IRequest<DeleteInstructionResult>;

    public record class DeleteInstructionResult : Result;

    public class DeleteInstructionHandler : IRequestHandler<DeleteInstructionCommand, DeleteInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public DeleteInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteInstructionResult> Handle(DeleteInstructionCommand request, CancellationToken ct)
        {
            var validator = new DeleteInstructionValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var entity = await _db.Set<Instruction>().FirstOrDefaultAsync(i => i.Id == request.Id, ct);
            if (entity is null)
            {
                return new() { ErrorMessage = "Nie znaleziono dawkowania." };
            }

            _db.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class DeleteInstructionValidator : AbstractValidator<DeleteInstructionCommand>
    {
        public DeleteInstructionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                    .WithMessage("Id dawkowania musi być liczbą dodatnią.");
        }
    }
}
