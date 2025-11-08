using MediatR;
using MyApp.Domain.Common;
using MyApp.Model;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Domain.Users.Commands;

public record CreateUserCommand(string Email, string Password,string Role) : IRequest<Result<User>>;


/// <summary>
/// Handles the <see cref="CreateUserCommand"/> by creating a new user account.
/// </summary>
/// <param name="request">
/// The command containing the user's email and password to create the account.
/// </param>
/// /// <returns>
/// A <see cref="Result{User}"/> that contains the newly created <see cref="User"/> 
/// if successful, or a failure result with an error message if the creation fails.
/// </returns>
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<User>>
{
    private static readonly string[] Allowed = { "Pharmacist", "Patient" };

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public CreateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return Result<User>.Fail("Email is already registered.");
        }

        var role = Allowed.FirstOrDefault(r => r.Equals(request.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null)
        {
            return Result<User>.Fail("Unsupported role.");
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
            return Result<User>.Fail(message);
        }

        var addRole = await _userManager.AddToRoleAsync(user, role);

        return Result<User>.Ok(user);
    }
}
