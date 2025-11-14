using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Queries
{
    public record class GetInstructionsQuery() : IRequest<GetInstructionsResult>;

    public record class GetInstructionsResult : HResult<IReadOnlyList<InstructionDto>>
    {
    }

    public class GetInstructionsHandler : IRequestHandler<GetInstructionsQuery, GetInstructionsResult>
    {
        private readonly ApplicationDbContext _db;

        public GetInstructionsHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<GetInstructionsResult> Handle(GetInstructionsQuery request, CancellationToken ct)
        {
            var result =  await _db.Set<Instruction>()
                .OrderBy(i => i.Code)
                .Select(i => new InstructionDto(i.Id, i.Code, i.Text))
                .ToListAsync(ct);

            return new() { Value = result };
        }
    }
}
