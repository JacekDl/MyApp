using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Medicines.Queries
{
    
    public record class GetMedicinesQuery(int Page = 1, int PageSize = 10) : IRequest<GetMedicinesResult>;

    public record class GetMedicinesResult : PagedResult<List<MedicineDto>>;

    public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, GetMedicinesResult>
    {
        private readonly ApplicationDbContext _db;
        public GetMedicinesQueryHandler(ApplicationDbContext db) => _db = db;

        public async Task<GetMedicinesResult> Handle(GetMedicinesQuery request, CancellationToken ct)
        {
            // zamiast walidacji i zwracania bledu: 
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

            var query = _db.Medicines
                .AsNoTracking()
                .OrderBy(m => m.Code)
                .ThenBy(m => m.Id);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MedicineDto(m.Id, m.Code, m.Name))
                .ToListAsync(ct);

            return new() { Value = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }
    }

    public class GetMedicinesValidator : AbstractValidator<GetMedicinesQuery>
    {
        public GetMedicinesValidator()
        {
           
        }
    }
}
