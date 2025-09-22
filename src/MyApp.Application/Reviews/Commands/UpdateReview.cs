using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Commands;

public record UpdateReviewCommand(string Number, string ReviewText) : IRequest<Result<bool>>;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;
    public UpdateReviewHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken ct)
    {
        var review = await _db.Reviews.SingleOrDefaultAsync(r => r.Number == request.Number, ct);
        if (review is null)
            return Result<bool>.Fail("Review not found.");
        if (review.Completed)
            return Result<bool>.Fail("Review already completed.");
        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
            return Result<bool>.Fail("Review expired");
        review.Response = request.ReviewText.Trim();
        review.Completed = true;
        _db.Update(review);
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
