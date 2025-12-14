using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Users;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Commands;

public record class UpdateReviewCommand(string Number, string ReviewText) : IRequest<UpdateReviewResult>;

public record class UpdateReviewResult : Result;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, UpdateReviewResult>
{
    private readonly ApplicationDbContext _db;
    public UpdateReviewHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UpdateReviewResult> Handle(UpdateReviewCommand request, CancellationToken ct)
    {
        var validator = new UpdateReviewCommandValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var review = await _db.Reviews
            .Include(r => r.Entries)
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        if (review.Completed)
        {
            return new() { ErrorMessage = "Opinia została już przesłana." };
        }

        if (review.DateCreated.AddDays(60) < DateTime.Now)
        {
            return new() { ErrorMessage = "Zalecenia już wygasły." };
        }

        var text = (request.ReviewText ?? string.Empty).Trim();

        review.Entries.Add(
            new Entry
            {
                UserId = null,
                Text = text,
                UserRole = UserRoles.Patient,
                ReviewId = review.Id
            });

        review.Completed = true;

        await _db.SaveChangesAsync(ct);
        return new();
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
