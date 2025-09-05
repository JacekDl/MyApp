using MyApp.Models;

namespace MyApp.Services;

public interface IReviewService
{
    Task<Review> CreateAsync(string? advice, CancellationToken ct = default);
    Task<Review?> GetPublicAsync(string number, CancellationToken ct = default);

    Task<bool> UpdatePublicAsync(string number, string? reviewText, CancellationToken ct = default);
}
