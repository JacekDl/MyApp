using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;

namespace MyApp.Application.Reviews.Commands;

public record UpdateReviewCommand(string Number, string ReviewText) : IRequest<Result<bool>>;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly IReviewRepository _repo;
    public UpdateReviewHandler(IReviewRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken ct)
    {
        var review = await _repo.GetReviewAsync(request.Number, ct);
        if (review is null)
            return Result<bool>.Fail("Review not found.");
        if (review.Completed)
            return Result<bool>.Fail("Review already completed.");
        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
            return Result<bool>.Fail("Review expired");
        review.Response = request.ReviewText.Trim();
        review.Completed = true;
        await _repo.UpdateAsync(review, ct);
        return Result<bool>.Ok(true);
    }
}
