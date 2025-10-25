using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public sealed record DeleteMedicineCommand(int Id) : IRequest<Result<bool>>;

    public sealed class DeleteMedicineHandler(ApplicationDbContext db) : IRequestHandler<DeleteMedicineCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteMedicineCommand request, CancellationToken ct)
        {
            var entity = await db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
                return Result<bool>.Fail("Medicine not found.");

            db.Remove(entity);
            await db.SaveChangesAsync(ct);
            return Result<bool>.Ok(true);
        }
    }
}
