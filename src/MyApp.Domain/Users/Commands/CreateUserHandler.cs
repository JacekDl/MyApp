using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record class CreateUserCommand(string Email, string Password, string Role) : IRequest<CreateUserResult>;

public record class CreateUserResult : Result<UserDto>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private static readonly string[] Allowed = { UserRoles.Pharmacist, UserRoles.Patient };

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public CreateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var validator = new CreateUserValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var email = request.Email.Trim();
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return new() { ErrorMessage = "Wybierz inny adres email." };
        }

        var role = Allowed.FirstOrDefault(r => r.Equals(request.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null)
        {
            return new() { ErrorMessage = "Nieobsługiwana rola." };
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var create = await _userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            var message = string.Join(";", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }

        var addRole = await _userManager.AddToRoleAsync(user, role);
        if (!addRole.Succeeded)
        {
            var message = string.Join(";", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }

        var userDto = new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            role,
            user.DisplayName ?? string.Empty,
            user.CreatedUtc
        );

        return new() { Value = userDto };
    }

    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        private static readonly string[] AllowedRoles = { UserRoles.Pharmacist, UserRoles.Patient };

        public CreateUserValidator()
        {
            RuleFor(x => x.Email)
                .Must(e => !string.IsNullOrWhiteSpace(e))
                    .WithMessage("Adres e-mail jest wymagany.")
                .MaximumLength(User.EmailMaxLength)
                    .WithMessage($"Adres e-mail nie może przekraczać {User.EmailMaxLength} znaków.")
                .EmailAddress()
                    .WithMessage("Nieprawidłowy adres e-mail.");

            RuleFor(x => x.Password)
                .Must(p => !string.IsNullOrWhiteSpace(p))
                    .WithMessage("Hasło jest wymagane.")
                .MinimumLength(User.PasswordMinLength)
                    .WithMessage($"Hasło musi zawierać co najmniej {User.PasswordMinLength} znaków.");

            RuleFor(x => x.Role)
                .Must(r => !string.IsNullOrWhiteSpace(r))
                    .WithMessage("Rola użytkownika jest wymagana.")
                .Must(role =>
                    AllowedRoles.Any(r =>
                        r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("Nieobsługiwana rola. Dozwolone role to: Pharmacist lub Patient.");
        }
    }
}
