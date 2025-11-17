using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;

namespace MyApp.Domain.Medicines.Queries
{
    
    public record class GetMedicinesQuery : IRequest<GetMedicinesResult>;

    public record class GetMedicinesResult : Result<IReadOnlyList<MedicineDto>>;

    public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, GetMedicinesResult>
    {
        private readonly ApplicationDbContext _db;
        public GetMedicinesQueryHandler(ApplicationDbContext db) => _db = db;

        public async Task<GetMedicinesResult> Handle(GetMedicinesQuery request, CancellationToken ct)
        {
            var validator = new GetMedicinesValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join("; ", validator.Errors.Select(e => e.ErrorMessage)) };
            }
            var result =  await _db.Medicines
                .OrderBy(m => m.Code)
                .Select(m => new MedicineDto(m.Id, m.Code, m.Name))
                .ToListAsync(ct);

            return new() { Value = result };
        }
    }

    public class GetMedicinesValidator : AbstractValidator<GetMedicinesQuery>
    {
        public GetMedicinesValidator()
        {
           
        }
    }
}
