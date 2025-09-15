using MediatR;
using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IReviewRepository
{
    Task CreateAsync(Review review, CancellationToken ct);
    Task<List<Review>> GetReviews(string? searchString, string? userId, bool? completed, CancellationToken ct);
}
