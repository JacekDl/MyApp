using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Queries
{
    public record class GetInstructionsQuery() : IRequest<GetInstructionsResult>;

    public record class GetInstructionsResult : Result<IReadOnlyList<InstructionDto>>;

    public class GetInstructionsHandler : IRequestHandler<GetInstructionsQuery, GetInstructionsResult>
    {
        private readonly ApplicationDbContext _db;

        public GetInstructionsHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<GetInstructionsResult> Handle(GetInstructionsQuery request, CancellationToken ct)
        {
            var validator = new GetInstructionsValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join("; ", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var result =  await _db.Set<Instruction>()
                .OrderBy(i => i.Code)
                .Select(i => new InstructionDto(i.Id, i.Code, i.Text))
                .ToListAsync(ct);

            return new() { Value = result };
        }
    }

    public class GetInstructionsValidator : AbstractValidator<GetInstructionsQuery>
    {
        public GetInstructionsValidator()
        {

        }
    }
}
