using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record UpdateUserPasswordCommand(string Id, string CurrentPassword, string NewPassword) : IRequest<Result<User>>;

public class UpdateUserPasswordHandler : IRequestHandler<UpdateUserPasswordCommand, Result<User>>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UpdateUserPasswordHandler(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<User>> Handle(UpdateUserPasswordCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<User>.Fail("User not found");

        var change = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!change.Succeeded)
        {
            var message = string.Join("; ", change.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result<User>.Fail("Wrong password");
        }

        await _signInManager.RefreshSignInAsync(user);
        return Result<User>.Ok(user);
    }
}