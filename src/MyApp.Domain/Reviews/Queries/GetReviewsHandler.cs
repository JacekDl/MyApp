using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Queries;

public record class GetReviewsQuery(string? SearchTxt, string? CurrentUserId, bool? Completed, string? UserEmail = null) 
    : IRequest<GetReviewsResult>;

public record class GetReviewsResult : Result<IReadOnlyList<ReviewDto>>;

public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, GetReviewsResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public GetReviewsHandler(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }
    public async Task<GetReviewsResult> Handle(GetReviewsQuery request, CancellationToken ct)
    {
        string? effectiveUserId = request.CurrentUserId;

        if(string.IsNullOrWhiteSpace(effectiveUserId) && !string.IsNullOrWhiteSpace(request.UserEmail))
        {
            var user = await _userManager.FindByEmailAsync(request.UserEmail.Trim());
            effectiveUserId = user?.Id;
        }

        var query = _db.Reviews
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTxt))
        {
            var pattern = $"%{request.SearchTxt.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Entries
                .OrderBy(e => e.CreatedUtc)
                .Select(e => e.Text)
                .FirstOrDefault() ?? string.Empty, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.CurrentUserId))
        {
            query = query.Where(r => r.PharmacistId == request.CurrentUserId || r.PatientId == request.CurrentUserId);
        }

        if (request.Completed.HasValue)
        {
            query = query.Where(r => r.Completed == request.Completed.Value);
        }

        var viewerId = effectiveUserId ?? string.Empty;

        var rows = await query
            .Select(r => new
            {
                r.Id,
                r.Number,
                r.PharmacistId,
                r.PatientId,
                r.Completed,
                r.DateCreated,
                FirstEntryText = r.Entries
                .OrderBy(e => e.CreatedUtc)
                .Select(e => e.Text)
                .FirstOrDefault() ?? string.Empty,

                LatestEntryUtc = r.Entries
                .OrderByDescending(e => e.CreatedUtc)
                .Select(e => (DateTime?)e.CreatedUtc)
                .FirstOrDefault(),

                IsNewForViewer = viewerId == r.PharmacistId ? r.PatientModified : r.PharmacistModified
            })
            .OrderByDescending(x => x.IsNewForViewer)
            .ThenByDescending(x => x.LatestEntryUtc)
            .ToListAsync(ct);

        var result = rows
            .Select(r =>
                 new ReviewDto(
                    r.Id,
                    r.PharmacistId!,
                    r.Number,
                    r.DateCreated,
                    r.FirstEntryText,
                    string.Empty,
                    r.Completed,
                    r.IsNewForViewer))
            .ToList();

        return new() { Value = result };
    }
}
