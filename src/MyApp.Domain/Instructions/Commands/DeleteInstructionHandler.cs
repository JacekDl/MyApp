using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record class DeleteInstructionCommand(int Id) : IRequest<DeleteInstructionResult>;

    public record class DeleteInstructionResult : HResult
    {
    }

    public class DeleteInstructionHandler : IRequestHandler<DeleteInstructionCommand, DeleteInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public DeleteInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteInstructionResult> Handle(DeleteInstructionCommand request, CancellationToken ct)
        {
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
}
