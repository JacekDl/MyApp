using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;


namespace MyApp.Domain.Medicines.Queries    
{
    public sealed record GetMedicineQuery(int Id) : IRequest<Result<MedicineDto>>;

    public sealed class GetMedicineHandler(ApplicationDbContext db) : IRequestHandler<GetMedicineQuery, Result<MedicineDto>>
    {
        public async Task<Result<MedicineDto>> Handle(GetMedicineQuery request, CancellationToken ct)
        {
            var result = await db.Set<Medicine>()
                .Where(m => m.Id == request.Id)
                .Select(m => new MedicineDto(m.Id, m.Code, m.Name))
                .FirstOrDefaultAsync(ct);

            if (result is null)
            {
                return Result<MedicineDto>.Fail("Nie znaleziono leku o podanym Id.");
            }
            return Result<MedicineDto>.Ok(result);
        }

    }
}
