using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Instructions.Queries
{
    public record class GetInstructionsQuery(int Page = 1, int PageSize = 10) : IRequest<GetInstructionsResult>;

    public record class GetInstructionsResult : PagedResult<List<InstructionDto>>;

    public class GetInstructionsHandler : IRequestHandler<GetInstructionsQuery, GetInstructionsResult>
    {
        private readonly ApplicationDbContext _db;

        public GetInstructionsHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<GetInstructionsResult> Handle(GetInstructionsQuery request, CancellationToken ct)
        {
            // zamiast validacji i zwracania bledu zmien Page i PageSize
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

            var query = _db.Instructions
                .AsNoTracking()
                .OrderBy(m => m.Code)
                .ThenBy(m => m.Id);

            var totalCount = await query.CountAsync(ct);

            var result =  await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InstructionDto(i.Id, i.Code, i.Text))
                .ToListAsync(ct);

            return new() { Value = result, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }
    }

    public class GetInstructionsValidator : AbstractValidator<GetInstructionsQuery>
    {
        public GetInstructionsValidator()
        {

        }
    }
}
