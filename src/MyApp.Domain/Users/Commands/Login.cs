using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record LoginCommand(string Email, string Password, bool RememberMe) : IRequest<LoginResult>;

public enum LoginStatus { Succeeded, WrongCredentials, NotAllowed, LockedOut}

public sealed record LoginResult(LoginStatus Status, string? Message = null, string? UserId = null, string? Role = null);

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
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new(LoginStatus.WrongCredentials, "Wrong email or password.");

        if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            return new(LoginStatus.NotAllowed, "Please confirm your email before logging in.");

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var effectiveRole = roles.FirstOrDefault() ?? user.Role;
            return new(LoginStatus.Succeeded, UserId: user.Id, Role: effectiveRole);
        }
        if (result.IsLockedOut)
            return new(LoginStatus.LockedOut, "Account locked. Try again later.");

        if (result.IsNotAllowed)
            return new(LoginStatus.NotAllowed, "Login not allowed. Please confirm your email.");

        return new(LoginStatus.WrongCredentials, "Wrong email or password");
    }

}
