using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;


namespace MyApp.Domain.Instructions.Queries
{
    public record class GetInstructionQuery(int Id) : IRequest<GetInstructionResult>;

    public record class GetInstructionResult : Result<InstructionDto>;

    public class GetInstructionHandler : IRequestHandler<GetInstructionQuery, GetInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public GetInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<GetInstructionResult> Handle(GetInstructionQuery request, CancellationToken ct)
        {
            var validator = new GetInstructionValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var result = await _db.Set<Instruction>()
                .Where(m => m.Id == request.Id)
                .Select(m => new InstructionDto(m.Id, m.Code, m.Text))
                .FirstOrDefaultAsync(ct);

            if (result is null)
            {
                return new() { ErrorMessage = "Nie znaleziono dawkowania o podanym Id." };
            }
            return new() { Value = result };
        }

    }

    public class GetInstructionValidator : AbstractValidator<GetInstructionQuery>
    {
        public GetInstructionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id dawkowania musi być dodatnie.");
        }
    }
}