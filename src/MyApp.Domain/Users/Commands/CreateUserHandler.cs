using MediatR;
using MyApp.Domain.Common;
using MyApp.Model;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Domain.Users.Commands;

public record class CreateUserCommand(string Email, string Password,string Role) : IRequest<CreateUserResult>;

public record class CreateUserResult : Result<User>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private static readonly string[] Allowed = { "Pharmacist", "Patient" };

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public CreateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return new() { ErrorMessage = "Podany email jest już zarejestrowany." };
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
            Role = request.Role,
            EmailConfirmed = false
        };

        var create = await _userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            var message = string.Join(";", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }

        var addRole = await _userManager.AddToRoleAsync(user, role);

        return new();
    }
}
