using Microsoft.EntityFrameworkCore;
using MyApp.Infrastructure;
using MyApp.Models;
using MyApp.Domain;
using System.Security.Cryptography;

namespace MyApp.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;

    public ReviewService(ApplicationDbContext db) => _db = db;

    public async Task<Review> CreateAsync(int userId, string? advice, CancellationToken ct = default)
    {
        string number;
        do
        {
            number = GenerateDigits(10);
        }
        while(_db.Reviews.Any(r => r.Number == number));

        var entity = new Review
        {
            Advice = advice!,
            Number = number,
            Completed = false,
            CreatedByUserId = userId
        };

        _db.Reviews.Add(entity);

        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private static string GenerateDigits(int digits)
    {
        var chars = new char[digits];
        for (int i = 0; i < digits; i++)
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        return new string(chars);
    }

    public async Task<Review?> GetPublicAsync(string number, CancellationToken ct = default)
    {
        return await _db.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Number == number, ct);
    }

    public async Task<bool> UpdatePublicAsync(string number, string? reviewText, CancellationToken ct = default)
    {
        var entity = await _db.Reviews.FirstOrDefaultAsync(r => r.Number == number, ct);
        
        if (entity is null)
        {
            return false;
        }

        entity.ReviewText = reviewText;
        entity.Completed = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Review>> GetByCreatorAsync(int userId, string? searchTxt, bool? completed, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Where(r => r.CreatedByUserId == userId);

        if(!string.IsNullOrWhiteSpace(searchTxt))
        {
            var pattern = $"%{searchTxt.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Advice ?? string.Empty, pattern) ||
                EF.Functions.Like(r.ReviewText ?? string.Empty, pattern));
        };

        if(completed.HasValue)
        {
            query = query.Where(r => r.Completed == completed.Value);
        }

        return await query
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReviewListItem>> GetReviewsAsync(string? searchTxt, string? userId, bool? completed, CancellationToken ct)
    {
        var query = _db.Reviews
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTxt))
        {
            var pattern = $"%{searchTxt.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Advice ?? string.Empty, pattern) ||
                EF.Functions.Like(r.ReviewText ?? string.Empty, pattern));
        };

        if (!string.IsNullOrWhiteSpace(userId))
        {
            if(int.TryParse(userId, out var uid))
            {
                query = query.Where(r => r.CreatedByUserId == uid);
            }
        }

        if (completed.HasValue)
        {
            query = query.Where(r => r.Completed == completed.Value);
        };

        return await query
            .OrderByDescending(r => r.DateCreated)
            .Select(r => new ReviewListItem
            {
                Id = r.Id,
                CreatedByUserId = r.CreatedByUserId,
                DateCreated = r.DateCreated,
                Advice = r.Advice,
                ReviewText = r.ReviewText!,
                Completed = r.Completed
            })
            .ToListAsync(ct);
    }
}