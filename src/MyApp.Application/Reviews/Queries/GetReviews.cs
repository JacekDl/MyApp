using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Data;
using MyApp.Domain;

namespace MyApp.Application.Reviews.Queries;

public record GetReviewsQuery(string? SearchTxt, string? UserId, bool? Completed, string? UserEmail = null) : IRequest<List<ReviewDto>>;

public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, List<ReviewDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public GetReviewsHandler(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }
    public async Task<List<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken ct)
    {
        string? effectiveUserId = request.UserId;

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

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(r => r.PharmacistId == request.UserId);
        }

        if (request.Completed.HasValue)
        {
            query = query.Where(r => r.Completed == request.Completed.Value);
        }

        var list = await query
            .OrderByDescending(r => r.DateCreated)
            .Select(r => new
            {
                r.Id,
                r.PharmacistId,
                r.Number,
                r.DateCreated,
                r.Completed,
                FirstEntryText = r.Entries
                    .OrderBy(e => e.CreatedUtc)
                    .Select(e => e.Text)
                    .FirstOrDefault() ?? string.Empty
            })
            .ToListAsync(ct);

        return list
            .Select(r =>
                 new ReviewDto(
                    r.Id,
                    r.PharmacistId,
                    r.Number,
                    r.DateCreated,
                    r.FirstEntryText,
                    string.Empty,
                    r.Completed))
            .ToList();
    }
}
