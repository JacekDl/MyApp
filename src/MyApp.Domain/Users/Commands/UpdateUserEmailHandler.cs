using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{

    public record class UpdateUserEmailCommand(string Id, string Email, string Password) : IRequest<UpdateUserEmailResult>;

    public record class UpdateUserEmailResult : Result<User>;

    public class UpdateUserEmailHandler : IRequestHandler<UpdateUserEmailCommand, UpdateUserEmailResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UpdateUserEmailHandler(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<UpdateUserEmailResult> Handle(UpdateUserEmailCommand request, CancellationToken ct)
        {
            var validator = new UpdateUserEmailValidator().Validate(request);
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
                return new() { ErrorMessage = "To konto loguje się przez Google, więc nie możesz zmienić adresu e-mail." };
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new() { ErrorMessage = "Nieprawidłowe hasło." };
            }

            var newEmail = request.Email.Trim();
            var existingWithEmail = await _userManager.FindByEmailAsync(newEmail);
            if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
            {
                return new() { ErrorMessage = "Podany email jest już zarejestrowany." };
            }

            var setEmail = await _userManager.SetEmailAsync(user, newEmail);
            if (!setEmail.Succeeded)
            {
                var message = string.Join(";", setEmail.Errors.Select(e => $"{e.Code}: {e.Description}"));
                return new() { ErrorMessage = message };
            }

            user.UserName = newEmail;
            user.EmailConfirmed = false;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                var message = string.Join("; ", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
                return new() { ErrorMessage = message };
            }

            await _signInManager.RefreshSignInAsync(user);
            return new() { Value = user };
        }
    }

    public class UpdateUserEmailValidator : AbstractValidator<UpdateUserEmailCommand>
    {
        public UpdateUserEmailValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");

            RuleFor(x => x.Email)
                .Must(e => !string.IsNullOrWhiteSpace(e))
                    .WithMessage("Adres e-mail nie może być pusty.")
                .EmailAddress()
                    .WithMessage("Proszę wprowadzić prawidłowy adres e-mail.")
                .MaximumLength(User.EmailMaxLength)
                    .WithMessage($"Adres e-mail nie może być dłuższy niż {User.EmailMaxLength} znaków.");

            RuleFor(x => x.Password)
                .Must(p => !string.IsNullOrWhiteSpace(p))
                    .WithMessage("Hasło nie może być puste.")
                .MaximumLength(User.PasswordMaxLength)
                    .WithMessage("Hasło jest zbyt długie.");
        }
    }
}
