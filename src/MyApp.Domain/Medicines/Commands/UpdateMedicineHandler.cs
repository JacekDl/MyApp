using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public record UpdateMedicineCommand(int Id, string Code, string Name) : IRequest<Result<bool>>;

    public class UpdateMedicineHandler : IRequestHandler<UpdateMedicineCommand, Result<bool>>
    {
        private readonly ApplicationDbContext _db;

        public UpdateMedicineHandler( ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<bool>> Handle(UpdateMedicineCommand request, CancellationToken ct)
        {
            var entity = await _db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
            {
                return Result<bool>.Fail("Medicine not found.");
            }
            entity.Code = (request.Code ?? string.Empty).Trim().ToUpper();
            var trimmedName = (request.Name ?? string.Empty).Trim();
            trimmedName = trimmedName.Length switch
            {
                0 => "",
                1 => trimmedName.ToUpper(),
                _ => char.ToUpper(trimmedName[0]) + trimmedName[1..].ToLower()
            };
            entity.Name = trimmedName;

            await _db.SaveChangesAsync(ct);
            return Result<bool>.Ok(true);
        }
    }
}
