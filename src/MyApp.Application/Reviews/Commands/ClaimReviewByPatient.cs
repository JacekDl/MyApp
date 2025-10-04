using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Commands;

public record ClaimReviewByPatientCommand(string Number, string PatientId) : IRequest<Result<bool>>;

public class ClaimReviewByPatientHandler : IRequestHandler<ClaimReviewByPatientCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;

    public ClaimReviewByPatientHandler( ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(ClaimReviewByPatientCommand request, CancellationToken ct)
    {
        var review = await _db.Reviews.SingleOrDefaultAsync(r => r.Number == request.Number);

        if (review is null)
        {
            return Result<bool>.Fail("Review not found.");
        }

        if (review.Completed)
        {
            return Result<bool>.Fail("Review already completed.");
        }

        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
        {
            return Result<bool>.Fail("Review expired.");
        }

        if (review.PatientId is not null && review.PatientId != request.PatientId)
        {
            return Result<bool>.Fail("This review is already assigned to another patient.");
        }

        review.PatientId = request.PatientId;
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
