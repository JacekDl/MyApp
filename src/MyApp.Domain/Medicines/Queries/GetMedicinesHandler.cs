using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Data;

namespace MyApp.Domain.Medicines.Queries
{
    
    public sealed record GetMedicinesQuery : IRequest<IReadOnlyList<MedicineDto>>;

    public sealed class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, IReadOnlyList<MedicineDto>>
    {
        private readonly ApplicationDbContext _db;
        public GetMedicinesQueryHandler(ApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<MedicineDto>> Handle(GetMedicinesQuery request, CancellationToken ct)
        {
            return await _db.Medicines
                .OrderBy(m => m.Code)
                .Select(m => new MedicineDto(m.Id, m.Code, m.Name))
                .ToListAsync(ct);
        }
    }
}
