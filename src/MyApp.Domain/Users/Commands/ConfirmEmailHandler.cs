using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{

    public record class ConfirmEmailCommand(string UserId, string Token) : IRequest<ConfirmEmailResult>;

    public record class ConfirmEmailResult : Result;

    public class ConfirmEmailHandler(UserManager<User> userManager) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
    {
        public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken ct)
        {

            var validator = new ConfirmEmailValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join("; ", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            if (user.EmailConfirmed)
            {
                return new();
            }

            var result = await userManager.ConfirmEmailAsync(user, request.Token);

            return new();
        }
    }

    public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Id użytkownika jest wymagane.");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token jest wymagany.");
        }
    }
}