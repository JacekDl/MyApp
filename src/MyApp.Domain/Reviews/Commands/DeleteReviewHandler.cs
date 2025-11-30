using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands
{
    public record class DeleteReviewCommand(int Id) : IRequest<DeleteReviewResult>;

    public record class DeleteReviewResult : Result;

    public class DeleteReviewHandler: IRequestHandler<DeleteReviewCommand, DeleteReviewResult>
    {
        private readonly ApplicationDbContext _db;

        public DeleteReviewHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<DeleteReviewResult> Handle(DeleteReviewCommand request, CancellationToken ct)
        {
            var validator = new DeleteReviewCommandValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            if (review is null)
                return new() { ErrorMessage = "Nie znaleziono zaleceń." };

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync(ct);
            return new();

        }
    }

    public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
    {
        public DeleteReviewCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Nieprawidłowy identyfikator zaleceń.");
        }
    }
}
