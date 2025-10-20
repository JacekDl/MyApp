using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Commands;

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
        var review = await _db.Reviews
            .Include(r => r.Entries)
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
            return Result<bool>.Fail("Review not found.");

        if (review.Completed)
            return Result<bool>.Fail("Review has been already submitted.");

        if (review.DateCreated.AddDays(7) < DateTime.Now)
            return Result<bool>.Fail("Review has expired.");

        var text = (request.ReviewText ?? string.Empty).Trim();

        review.Entries.Add(new Entry
        {
            UserId = null,
            Text = text,
            ReviewId = review.Id
        });

        review.Completed = true;

        await _db.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
