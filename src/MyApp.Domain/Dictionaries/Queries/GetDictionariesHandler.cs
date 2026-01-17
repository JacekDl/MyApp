using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Dictionaries.Queries
{

    public record class GetDictionariesQuery() : IRequest<GetDictionariesResult>;

    public record class GetDictionariesResult : Result<DictionaryDto>;

    public class GetDictionariesHandler : IRequestHandler<GetDictionariesQuery, GetDictionariesResult>
    {
        private readonly ApplicationDbContext _db;

        public GetDictionariesHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetDictionariesResult> Handle(GetDictionariesQuery request, CancellationToken ct)
        {
            var instructions = await _db.Instructions
                .AsNoTracking()
                .Select(i => new { i.Code, i.Text })
                .ToListAsync(ct);

            var medicines = await _db.Medicines
                .AsNoTracking()
                .Select(m => new { m.Code, m.Name })
                .ToListAsync(ct);

            var dictionary =  new DictionaryDto
            {
                InstructionMap  = instructions.ToDictionary(x => x.Code, x => x.Text, StringComparer.OrdinalIgnoreCase),
                MedicineMap     = medicines.ToDictionary(x => x.Code, x => x.Name, StringComparer.OrdinalIgnoreCase),
            };

            return new() { Value = dictionary };
        }
    }
}
