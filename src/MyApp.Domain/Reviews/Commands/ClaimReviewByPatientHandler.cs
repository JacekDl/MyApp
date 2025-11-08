using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands;

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

public class ClaimReviewByPatientCommandValidator : AbstractValidator<ClaimReviewByPatientCommand>
{
    private const int ReviewNumberLen = 16;

    public ClaimReviewByPatientCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Numer jest wymagany.")
            .Length(ReviewNumberLen).WithMessage($"Numer musi mieć {ReviewNumberLen} znaków.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Numer musi się składać z liter i cyfr.");

        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Id użytkownika jest wymagane.");
    }
}
