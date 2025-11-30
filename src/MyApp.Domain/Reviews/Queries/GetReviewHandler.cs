using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;

namespace MyApp.Domain.Reviews.Queries;

public record class GetReviewQuery(string Number) : IRequest<GetReviewResult>;

public record class GetReviewResult : Result<ReviewDto>;

public class GetReviewHandler : IRequestHandler<GetReviewQuery, GetReviewResult>
{

    private readonly ApplicationDbContext _db;

    public GetReviewHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<GetReviewResult> Handle(GetReviewQuery request, CancellationToken ct)
    {
        var validator = new GetReviewValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

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
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        if (review.Completed)
        {
            return new() { ErrorMessage = "Wykorzystano już kod zaleceń." };
        }

        if (review.DateCreated.AddDays(60) < DateTime.UtcNow)
        {
            return new () {ErrorMessage = "Minął już termin wykorzystania kodu."};
        }

        var dto = new ReviewDto(
            review.Id,
            review.PharmacistId!,
            review.Number,
            review.DateCreated,
            review.FirstEntryText ?? string.Empty,
            string.Empty,
            review.Completed,
            true
            );

        return new() { Value = dto  };
    }

    public class GetReviewValidator : AbstractValidator<GetReviewQuery>
    {
        public GetReviewValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                    .WithMessage("Numer tokenu jest wymagany.")
                .Length(16)
                    .WithMessage("Numer tokenu musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer tokenu może zawierać tylko litery i cyfry.");
        }
    }
}
