using MediatR;
using MyApp.Application.Common;
using MyApp.Domain;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Application.Users.Commands;

public record CreateUserCommand(string Email, string Password) : IRequest<Result<User>>;


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
            return Result<User>.Fail("Email is already registered.");

        var user = new User
        {
            UserName = email,
            Email = email,
            Role = "User"
        };

        var create = await _userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            var message = string.Join(";", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result<User>.Fail(message);
        }

        const string defaultRole = "User";
        if (!await _roleManager.RoleExistsAsync(defaultRole))
            await _roleManager.CreateAsync(new IdentityRole(defaultRole));

        await _userManager.AddToRoleAsync(user, defaultRole);

        return Result<User>.Ok(user);
    }
}
