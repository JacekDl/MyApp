using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record class RemoveUserCommand(string Id) : IRequest<RemoveUserResult>;

public record class RemoveUserResult : Result;

public class RemoveUserHandler : IRequestHandler<RemoveUserCommand, RemoveUserResult>
{
    private readonly UserManager<User> _userManager;
    public RemoveUserHandler(UserManager<User> userManager)
    {
        _userManager = userManager;

    }
    public async Task<RemoveUserResult> Handle(RemoveUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new() { ErrorMessage = "Nie znaleziono użytkownika." };
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var error = string.Join(";", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = error };
        }

        return new();
    }
}