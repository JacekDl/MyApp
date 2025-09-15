using Microsoft.EntityFrameworkCore;
using MyApp.Application.Abstractions;
using MyApp.Domain;

namespace MyApp.Infrastructure;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public ReviewRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(Review review, CancellationToken ct)
    {
        _db.Add(review);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Review>> GetReviews(string? searchString, string? userId, bool? completed, CancellationToken ct)
    {
        var query = _db.Reviews
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var pattern = $"%{searchString.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Advice ?? string.Empty, pattern) ||
                EF.Functions.Like(r.ReviewText ?? string.Empty, pattern));
        }
        ;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            if (int.TryParse(userId, out var uid))
            {
                query = query.Where(r => r.CreatedByUserId == uid);
            }
        }

        if (completed.HasValue)
        {
            query = query.Where(r => r.Completed == completed.Value);
        }
        
        return await query
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync(ct);
    }
}

