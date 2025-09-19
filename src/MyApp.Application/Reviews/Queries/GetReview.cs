using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;

namespace MyApp.Application.Reviews.Queries;

public record GetReviewQuery(string number) : IRequest<Result<ReviewDto>>;

public class GetReviewHandler : IRequestHandler<GetReviewQuery, Result<ReviewDto>>
{

    private readonly IReviewRepository _repo;
    public GetReviewHandler(IReviewRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<ReviewDto>> Handle(GetReviewQuery request, CancellationToken ct)
    {
        var review = await _repo.GetReviewAsync(request.number, ct);
        if (review is null)
            return Result<ReviewDto>.Fail("Review not found");

        if (review.Completed)
            return Result<ReviewDto>.Fail("Review already completed yet");

        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
            return Result<ReviewDto>.Fail("Review expired");

        var dto = new ReviewDto(
            review.Id,
            review.CreatedByUserId,
            review.Number,
            review.DateCreated,
            review.Advice,
            review.Response!,
            review.Completed
            );

        return Result<ReviewDto>.Ok(dto);
    }
}
