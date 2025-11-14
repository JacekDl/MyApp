using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public record class DeleteMedicineCommand(int Id) : IRequest<DeleteMedicineResult>;

    public record class DeleteMedicineResult : HResult
    {

    }

    public class DeleteMedicineHandler : IRequestHandler<DeleteMedicineCommand, DeleteMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public DeleteMedicineHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteMedicineResult> Handle(DeleteMedicineCommand request, CancellationToken ct)
        {
            var entity = await _db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
                return new() { ErrorMessage = "Medicine not found." };

            _db.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }
}
