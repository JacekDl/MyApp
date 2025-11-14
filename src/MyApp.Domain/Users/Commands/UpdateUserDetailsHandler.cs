using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record class UpdateUserDetailsCommand(string Id, string? Name) : IRequest<UpdateUserDetailsResult>;

public record class UpdateUserDetailsResult : Result<User>;


public class UpdateUserDetailsHandler : IRequestHandler<UpdateUserDetailsCommand, UpdateUserDetailsResult>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserDetailsHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    public async Task<UpdateUserDetailsResult> Handle(UpdateUserDetailsCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new() { ErrorMessage = "Nie znaleziono użytkownika." };
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            user.DisplayName = request.Name.Trim();
        }
            
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var message = string.Join("; ", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }

        return new() { Value = user };
    }
}