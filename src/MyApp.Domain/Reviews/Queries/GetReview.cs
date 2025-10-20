using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Queries;

public record GetReviewQuery(string Number) : IRequest<Result<ReviewDto>>;

public class GetReviewHandler : IRequestHandler<GetReviewQuery, Result<ReviewDto>>
{

    private readonly ApplicationDbContext _db;
    
    public GetReviewHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<Result<ReviewDto>> Handle(GetReviewQuery request, CancellationToken ct)
    {
        var review = await _db.Reviews
           .Where(r => r.Number == request.Number)
           .Select(x => new
           {
               x.Id,
               x.PharmacistId,
               x.Number,
               x.DateCreated,
               x.Completed,
               FirstEntryText = x.Entries
               .OrderBy(e => e.CreatedUtc)
               .Select(e => e.Text)
               .FirstOrDefault()
           })
           .SingleOrDefaultAsync(ct);

        if (review is null)
            return Result<ReviewDto>.Fail("Review not found");

        if (review.Completed)
            return Result<ReviewDto>.Fail("Review already completed yet");

        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
            return Result<ReviewDto>.Fail("Review expired");

        var dto = new ReviewDto(
            review.Id,
            review.PharmacistId,
            review.Number,
            review.DateCreated,
            review.FirstEntryText ?? string.Empty,
            string.Empty,
            review.Completed,
            true
            );

        return Result<ReviewDto>.Ok(dto);
    }
}
