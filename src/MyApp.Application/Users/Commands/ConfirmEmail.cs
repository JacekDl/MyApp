using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record ConfirmEmailCommand(string UserId, string Token) : IRequest<bool>;

public class ConfirmEmailHandler(UserManager<User> userManager) : IRequestHandler<ConfirmEmailCommand, bool>
{
    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
            return false;

        if (user.EmailConfirmed)
            return true;

        var result = await userManager.ConfirmEmailAsync(user, request.Token);

        return result.Succeeded;
    }
}