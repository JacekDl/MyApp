using FluentValidation;
using MediatR;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands
{
    public record class CloseConversationCommand(string Number, string UserId) : IRequest<CloseConverstionResult>;

    public record class CloseConverstionResult : Result;

    public class CloseConversationHandler : IRequestHandler<CloseConversationCommand, CloseConverstionResult>
    {
        private readonly ApplicationDbContext _db;

        public CloseConversationHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CloseConverstionResult> Handle(CloseConversationCommand request, CancellationToken ct)
        {
            var validator = new CloseConversationValidator().Validate(request);

            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var review = _db.Reviews
                .SingleOrDefault(r => r.Number == request.Number);

            if (review is null)
            {
                return new() { ErrorMessage = "Nie znaleziono zaleceń." };
            }

            review.Completed = true;
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class CloseConversationValidator : AbstractValidator<CloseConversationCommand>
    {
        public CloseConversationValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Numer rozmowy jest wymagany.");
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Id użytkownika jest wymagane.");
        }
    }
}