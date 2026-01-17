using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Users.Queries
{
    public record class GetPendingPromotionsQuery() : IRequest<GetPendingPromotionsResult>;

    public record class GetPendingPromotionsResult() : Result<IReadOnlyList<PendingPromotionsDto>>; //TODO: zmienić na DTO


    public class GetPendingPromotionsHandler : IRequestHandler<GetPendingPromotionsQuery, GetPendingPromotionsResult>
    {
        private readonly ApplicationDbContext _db;

        public GetPendingPromotionsHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetPendingPromotionsResult> Handle(GetPendingPromotionsQuery request, CancellationToken ct)
        {
            const string pending = "Pending";

            var items = await _db.PharmacistPromotionRequests
                .AsNoTracking()
                .Where(pr => pr.Status == pending)
                .OrderByDescending(pr => pr.CreatedUtc)
                .Select(pr => new
                {
                    pr,
                    user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == pr.UserId)
                })
                .Select(x => new PendingPromotionsDto(
                    x.pr.Id,
                    x.pr.UserId,
                    x.user != null ? x.user.Email! : "(brak)",
                    x.user != null ? x.user.DisplayName : null,
                    x.pr.FirstName,
                    x.pr.LastName,
                    x.pr.NumerPWZF,
                    x.pr.Status,
                    x.pr.CreatedUtc
                ))
                .ToListAsync(ct);

            return new() { Value = items };
        }
    }
}
