using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Queries
{
    public sealed record GetInstructionsQuery() : IRequest<IReadOnlyList<InstructionDto>>;

    public sealed class GetInstructionsHandler(ApplicationDbContext db)
        : IRequestHandler<GetInstructionsQuery, IReadOnlyList<InstructionDto>>
    {
        public async Task<IReadOnlyList<InstructionDto>> Handle(GetInstructionsQuery request, CancellationToken ct)
        {
            return await db.Set<Instruction>()
                .OrderBy(i => i.Code)
                .Select(i => new InstructionDto(i.Id, i.Code, i.Text))
                .ToListAsync(ct);
        }
    }
}
