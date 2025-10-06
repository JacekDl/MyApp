using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record UpdateUserDetailsCommand(string Id, string? Name) : IRequest<Result<User>>;


public class UpdateUserDetailsHandler : IRequestHandler<UpdateUserDetailsCommand, Result<User>>
{
    private readonly UserManager<User> _userManager;
    //private readonly SignInManager<User> _signInManager;

    public UpdateUserDetailsHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
        //_signInManager = signInManager;
    }
    public async Task<Result<User>> Handle(UpdateUserDetailsCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<User>.Fail("User not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            user.DisplayName = request.Name.Trim();

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var message = string.Join("; ", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result<User>.Fail(message);
        }

        return Result<User>.Ok(user);
    }
}