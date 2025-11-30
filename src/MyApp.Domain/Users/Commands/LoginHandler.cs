using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record class LoginCommand(string Email, string Password, bool RememberMe) : IRequest<LoginResult>;

public record class LoginResult : Result<UserDto>;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public LoginHandler(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var validator = new LoginValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new() { ErrorMessage = "Nieprawidłowy email lub hasło." };
        }

        if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
        {
            return new() { ErrorMessage = "Potwierdź swój email przed zalogowaniem." };
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var effectiveRole = roles.FirstOrDefault() ?? user.Role;
            var userDto = new UserDto(
                user.Id,
                user.Email!,
                effectiveRole,
                "",
                user.CreatedUtc
            );
            return new() { Value = userDto };
        }
        if (result.IsLockedOut)
        {
            return new() { ErrorMessage = "Konto zablokowane. Spróbuj ponownie później." };
        }

        if (result.IsNotAllowed)
        {
            return new() { ErrorMessage = "Potwierdź swój email przed zalogowaniem." };
        }

        return new() { ErrorMessage = "Nieprawidłowy email lub hasło." };
    }

    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Adres e-mail jest wymagany.")
                .MaximumLength(256)
                    .WithMessage("Adres e-mail nie może przekraczać 256 znaków.")
                .EmailAddress()
                    .WithMessage("Nieprawidłowy adres e-mail.");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Hasło jest wymagane.");
        }
    }

}
