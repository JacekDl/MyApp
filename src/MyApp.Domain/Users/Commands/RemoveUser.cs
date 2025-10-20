using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record RemoveUserCommand(string Id) : IRequest<Result<bool>>;

public class RemoveUserHandler : IRequestHandler<RemoveUserCommand, Result<bool>>
{
    private readonly UserManager<User> _userManager;
    public RemoveUserHandler(UserManager<User> userManager)
    {
        _userManager = userManager;

    }
    public async Task<Result<bool>> Handle(RemoveUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<bool>.Fail("User not found");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result<bool>.Fail(error);
        }

        return Result<bool>.Ok(true);
    }
}