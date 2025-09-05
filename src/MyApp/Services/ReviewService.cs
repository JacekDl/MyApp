using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.Security.Cryptography;

namespace MyApp.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;

    public ReviewService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Review> CreateAsync(string? advice, CancellationToken ct = default)
    {
        string number;
        do
        {
            number = GenerateDigits(10);
        }
        while(_db.Reviews.Any(r => r.Number == number));

        var entity = new Review { Advice = advice!, Number = number, Completed = false };
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
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
