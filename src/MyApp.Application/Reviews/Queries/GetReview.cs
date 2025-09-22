using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Queries;

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
        var review = await _db.Reviews.SingleOrDefaultAsync(r => r.Number == request.Number, ct);

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
