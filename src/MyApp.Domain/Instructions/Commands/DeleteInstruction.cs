using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public sealed record DeleteInstructionCommand(int Id) : IRequest<Result<bool>>;

    public sealed class DeleteInstructionHandler(ApplicationDbContext db) : IRequestHandler<DeleteInstructionCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteInstructionCommand request, CancellationToken ct)
        {
            var entity = await db.Set<Instruction>().FirstOrDefaultAsync(i => i.Id == request.Id, ct);
            if (entity is null) return Result<bool>.Fail("Instruction not found.");

            db.Remove(entity);
            await db.SaveChangesAsync(ct);
            return Result<bool>.Ok(true);
        }
    }
}
