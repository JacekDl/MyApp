using MediatR;
using MyApp.Application.Abstractions;

namespace MyApp.Application.Reviews.Queries;

public record GetReviewsQuery(string? searchTxt, string? userId, bool? completed) : IRequest<List<ReviewDto>>;

public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _repo;

    public GetReviewsHandler(IReviewRepository repo)
    {
        _repo = repo;
    }
    public async Task<List<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken ct)
    {
        var list = await _repo.GetReviews(request.searchTxt, request.userId, request.completed, ct);

        return list
            .Select(r => new ReviewDto(
                r.Id,
                r.CreatedByUserId,
                r.Number,
                r.DateCreated,
                r.Advice,
                r.ReviewText!,
                r.Completed
                ))
            .ToList();
    }
}
