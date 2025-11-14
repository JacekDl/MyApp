using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
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
            var result = await _db.Set<Instruction>()
                .Where(m => m.Id == request.Id)
                .Select(m => new InstructionDto(m.Id, m.Code, m.Text))
                .FirstOrDefaultAsync(ct);

            if (result is null)
            {
                return new() { ErrorMessage = "Nie znaleziono leku o podanym Id." };
            }
            return new() { Value = result };
        }

    }
}