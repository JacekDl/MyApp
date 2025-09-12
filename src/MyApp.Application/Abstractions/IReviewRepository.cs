using MediatR;
using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IReviewRepository
{
    Task<List<Review>> GetReviews(string? searchString, string? userId, bool? completed, CancellationToken ct);
}
