using FluentValidation;
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

    public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
    {
        private const int NumberLen = 16;
        private const int MaxReviewLen = 200;

        public UpdateReviewCommandValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Numer tokenu jest wymagany.")
                .Length(NumberLen).WithMessage($"Numer tokenu musi mieć dokładnie {NumberLen} znaków.")
                .Matches("^[A-Za-z0-9]+$").WithMessage("Numer tokenu może zawierać tylko litery i cyfry.");

            RuleFor(x => x.ReviewText)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                    .WithMessage("Opinia nie może być pusta.")
                .Must(s => (s ?? string.Empty).Trim().Length <= MaxReviewLen)
                    .WithMessage($"Opinia nie może przekraczać {MaxReviewLen} znaków.");
        }
    }
}
