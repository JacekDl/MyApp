using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using static System.Net.Mime.MediaTypeNames;

namespace MyApp.Domain.Medicines.Commands
{

    public sealed record AddMedicineCommand(string Code, string Name) : IRequest<Result<bool>>;

    public sealed class AddMedicineHandler(ApplicationDbContext db) : IRequestHandler<AddMedicineCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AddMedicineCommand request, CancellationToken ct)
        {
            (var code, var text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Name);

            var exists = await db.Set<Medicine>()
                .AnyAsync(m => m.Code == code, ct);

            if (exists)
                return Result<bool>.Fail($"Kod '{code}' jest już przypisany do leku.");

            db.Add(new Medicine { Code = code, Name = text });
            await db.SaveChangesAsync(ct);

            return Result<bool>.Ok(true);
        }
    }
}

