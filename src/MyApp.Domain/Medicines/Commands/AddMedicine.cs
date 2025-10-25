﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Medicines.Commands
{
    public sealed record AddMedicineCommand(string Code, string Name) : IRequest<Result<bool>>;

    public sealed class AddMedicineHandler(ApplicationDbContext db) : IRequestHandler<AddMedicineCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AddMedicineCommand request, CancellationToken ct)
        {
            var trimmedCode = (request.Code ?? "").Trim();
            var trimmedName = (request.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(trimmedCode) || string.IsNullOrWhiteSpace(trimmedName))
                return Result<bool>.Fail("Code and Name are required.");

            var exists = await db.Set<Medicine>()
                .AnyAsync(m => m.Code == trimmedCode, ct);

            if (exists)
                return Result<bool>.Fail($"Medicine with code '{trimmedCode}' already exists.");

            db.Add(new Medicine { Code = trimmedCode, Name = trimmedName });
            await db.SaveChangesAsync(ct);

            return Result<bool>.Ok(true);
        }
    }
}
