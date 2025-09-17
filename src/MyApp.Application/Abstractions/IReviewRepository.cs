using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IReviewRepository
{
    Task CreateAsync(Review review, CancellationToken ct);
    Task<Review?> GetReviewAsync(string number, CancellationToken ct);
    Task<List<Review>> GetReviews(string? searchString, string? userId, bool? completed, CancellationToken ct);
    Task UpdateAsync(Review review, CancellationToken ct);
}
