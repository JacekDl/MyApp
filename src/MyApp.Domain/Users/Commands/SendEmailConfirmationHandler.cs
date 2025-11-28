using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record SendEmailConfirmationCommand(string UserId, string CallbackUrl) : IRequest;

public class SendEmailConfirmationHandler(UserManager<User> userManager, IEmailSender email) : IRequestHandler<SendEmailConfirmationCommand>
{
    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
            throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (user.EmailConfirmed)
            return;

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var url = $"{request.CallbackUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        var body = $"""
        <p>Potwierdź swój email klikając poniższy link:</p>
        <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Potwierdź swój email</a><p>
        <p>Link jest ważny przez 24 godziny.</p>
        """;

        await email.SendEmailAsync(user.Email!, "Potwierdź email", body, ct);
    }
}