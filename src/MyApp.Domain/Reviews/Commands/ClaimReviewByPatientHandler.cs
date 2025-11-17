using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands;

public record class ClaimReviewByPatientCommand(string Number, string PatientId) : IRequest<ClaimReviewByPatientResult>;

public record class ClaimReviewByPatientResult : Result;

public class ClaimReviewByPatientHandler : IRequestHandler<ClaimReviewByPatientCommand, ClaimReviewByPatientResult>
{
    private readonly ApplicationDbContext _db;

    public ClaimReviewByPatientHandler( ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimReviewByPatientResult> Handle(ClaimReviewByPatientCommand request, CancellationToken ct)
    {
        var validator = new ClaimReviewByPatientCommandValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join("; ", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var review = await _db.Reviews.SingleOrDefaultAsync(r => r.Number == request.Number);

        if (review is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        if (review.Completed)
        {
            return new() { ErrorMessage = "Zalecenia zostały już pobrane." };
        }

        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
        {
            return new() { ErrorMessage = "Upłynął termin ważności zaleceń." };
        }

        if (review.PatientId is not null && review.PatientId != request.PatientId)
        {
            return new() { ErrorMessage = "Zalecenia zostały już użyte." };
        }

        review.PatientId = request.PatientId;
        await _db.SaveChangesAsync(ct);
        return new();
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
