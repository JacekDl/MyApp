using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{

    public record class UpdateUserPasswordCommand(string Id, string CurrentPassword, string NewPassword) : IRequest<UpdateUserPasswordResult>;

    public record class UpdateUserPasswordResult : Result<User>;

    public class UpdateUserPasswordHandler : IRequestHandler<UpdateUserPasswordCommand, UpdateUserPasswordResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UpdateUserPasswordHandler(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<UpdateUserPasswordResult> Handle(UpdateUserPasswordCommand request, CancellationToken ct)
        {
            var validator = new UpdateUserPasswordValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            if (!await _userManager.HasPasswordAsync(user))
            {
                return new() { ErrorMessage = "To konto loguje się przez Google, więc nie możesz zmienić hasła." };
            }

            var change = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!change.Succeeded)
            {
                var message = "Nie udało się zmienić hasła. ";
                return new() { ErrorMessage = message };
            }

            await _signInManager.RefreshSignInAsync(user);
            return new() { Value = user };
        }
    }

    public class UpdateUserPasswordValidator : AbstractValidator<UpdateUserPasswordCommand>
    {
        public UpdateUserPasswordValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");

            RuleFor(x => x.CurrentPassword)
                .Must(p => !string.IsNullOrWhiteSpace(p))
                    .WithMessage("Podane hasło nie może być puste.")
                .MaximumLength(User.PasswordMaxLength)
                    .WithMessage("Podane hasło jest zbyt długie.");

            RuleFor(x => x.NewPassword)
                .Must(p => !string.IsNullOrWhiteSpace(p))
                    .WithMessage("Nowe hasło nie może być puste.")
                .MinimumLength(User.PasswordMinLength)
                    .WithMessage($"Nowe hasło musi mieć co najmniej {User.PasswordMinLength} znaków.")
                .MaximumLength(User.PasswordMaxLength)
                    .WithMessage("Nowe hasło jest zbyt długie.");
        }
    }
}