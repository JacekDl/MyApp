using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

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
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new() { ErrorMessage = "Nie znaleziono użytkownika." };
        }

        var change = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!change.Succeeded)
        {
            var message = string.Join("; ", change.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }

        await _signInManager.RefreshSignInAsync(user);
        return new() { Value = user };
    }
}