using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;

namespace MyApp.Application.Reviews.Commands;

public record UpdateReviewCommand(string number, string reviewText) : IRequest<Result<bool>>;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly IReviewRepository _repo;
    public UpdateReviewHandler(IReviewRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken ct)
    {
        var review = await _repo.GetReviewAsync(request.number, ct);
        if (review is null)
            return Result<bool>.Fail("Review not found");
        if (review.Completed)
            return Result<bool>.Fail("Review already completed yet");
        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
            return Result<bool>.Fail("Review expired");
        review.ReviewText = request.reviewText;
        review.Completed = true;
        await _repo.UpdateAsync(review, ct);
        return Result<bool>.Ok(true);
    }
}
