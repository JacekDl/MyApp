using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
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
                .NotEmpty()
                    .WithMessage("Adres e-mail jest wymagany.")
                .MaximumLength(256)
                    .WithMessage("Adres e-mail nie może przekraczać 256 znaków.")
                .EmailAddress()
                    .WithMessage("Nieprawidłowy adres e-mail.");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Hasło jest wymagane.")
                .MinimumLength(6)
                    .WithMessage("Hasło musi zawierać co najmniej 6 znaków.");

            RuleFor(x => x.Role)
                .NotEmpty()
                    .WithMessage("Rola użytkownika jest wymagana.")
                .Must(role =>
                    AllowedRoles.Any(r =>
                        r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("Nieobsługiwana rola. Dozwolone role to: Pharmacist lub Patient.");
        }
    }
}
