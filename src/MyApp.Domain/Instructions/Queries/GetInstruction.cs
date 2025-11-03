using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;


namespace MyApp.Domain.Instructions.Queries
{
    public sealed record GetInstructionQuery(int Id) : IRequest<Result<InstructionDto>>;

    public sealed class GetInstructionHandler(ApplicationDbContext db) : IRequestHandler<GetInstructionQuery, Result<InstructionDto>>
    {
        public async Task<Result<InstructionDto>> Handle(GetInstructionQuery request, CancellationToken ct)
        {
            var result = await db.Set<Instruction>()
                .Where(m => m.Id == request.Id)
                .Select(m => new InstructionDto(m.Id, m.Code, m.Text))
                .FirstOrDefaultAsync(ct);

            if (result is null)
            {
                return Result<InstructionDto>.Fail("Nie znaleziono leku o podanym Id.");
            }
            return Result<InstructionDto>.Ok(result);
        }

    }
}