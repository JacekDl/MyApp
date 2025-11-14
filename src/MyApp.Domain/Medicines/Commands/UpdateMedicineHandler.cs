using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public record class UpdateMedicineCommand(int Id, string Code, string Name) : IRequest<UpdateMedicineResult>;

    public record class UpdateMedicineResult : HResult
    {
    }

    public class UpdateMedicineHandler : IRequestHandler<UpdateMedicineCommand, UpdateMedicineResult>
    {
        private readonly ApplicationDbContext _db;

        public UpdateMedicineHandler( ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<UpdateMedicineResult> Handle(UpdateMedicineCommand request, CancellationToken ct)
        {
            var entity = await _db.Set<Medicine>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
            {
                return new() { ErrorMessage = "Nie znaleziono leku w bazie." };
            }

            var (code, name) = FormatStringHelper.FormatCodeAndText(request.Code, request.Name);

            var exists = await _db.Set<Medicine>()
                .FirstOrDefaultAsync(m => m.Code == code, ct);

            if (exists != null && exists.Id != request.Id)
            {
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };
            }

            entity.Code = code;
            entity.Name = name;

            await _db.SaveChangesAsync(ct);
            return new();
        }
    }
}
