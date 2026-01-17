using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{
    public record ResetUserPasswordByTokenCommand(string UserId, string Token, string NewPassword) : IRequest<ResetUserPasswordByTokenResult>;

    public record ResetUserPasswordByTokenResult : Result;


    public class ResetUserPasswordByTokenHandler : IRequestHandler<ResetUserPasswordByTokenCommand, ResetUserPasswordByTokenResult>
    {
        private readonly UserManager<User> _userManager;

        public ResetUserPasswordByTokenHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResetUserPasswordByTokenResult> Handle(ResetUserPasswordByTokenCommand request, CancellationToken cancellationToken)
        {
            var validator = new ResetUserPasswordByTokenValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            var identityResult = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                return new() { ErrorMessage = $"Nie udało się zresetować hasła: {errors}" };
            }

            return new();
        }
    }

    public class ResetUserPasswordByTokenValidator : AbstractValidator<ResetUserPasswordByTokenCommand>
    {
        public ResetUserPasswordByTokenValidator()
        {
            RuleFor(x => x.UserId)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");

            RuleFor(x => x.Token)
                .Must(t => !string.IsNullOrWhiteSpace(t))
                    .WithMessage("Token resetu hasła nie może być pusty.")
                .MaximumLength(2048)
                    .WithMessage("Token resetu hasła jest nieprawidłowy.");

            RuleFor(x => x.NewPassword)
                .Must(p => !string.IsNullOrWhiteSpace(p))
                    .WithMessage("Nowe hasło nie może być puste.")
                .MinimumLength(User.PasswordMinLength)
                    .WithMessage($"Nowe hasło musi mieć co najmniej {User.PasswordMinLength} znaków.")
                .MaximumLength(User.PasswordMaxLength)
                    .WithMessage($"Nowe hasło nie może mieć więcej niż {User.PasswordMaxLength} znaków.");
        }
    }
}
