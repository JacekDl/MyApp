using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record SendEmailConfirmationCommand(string UserId, string CallbackUrl) : IRequest;

public class SendEmailConfirmationHandler(UserManager<User> userManager, IEmailSender email) : IRequestHandler<SendEmailConfirmationCommand>
{
    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.UserId);

        if (user is null)
            throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (user.EmailConfirmed)
            return;

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var url = $"{request.CallbackUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        var body = $"""
        //    <p>Confirm your email by clicking the link below:</p>
        //    <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Confirm my email</a><p>
        //    <p>This link expires in 24 hours.</p>
        """;

        await email.SendEmailAsync(user.Email!, "Confirm your email", body, ct);
    }


    //public async Task Handle(SendEmailConfirmationCommand request, CancellationToken ct)
    //{
    //    var user = await repo.GetByIdAsync(request.UserId, ct);
    //    if (user is null)
    //    {
    //        throw new KeyNotFoundException($"User {request.UserId} not found.");
    //    }

    //    if (user.EmailConfirmed)
    //        return;

    //    var token = EmailToken.CreateToken();
    //    var hash = EmailToken.Hash(token);
    //    var expires = DateTimeOffset.UtcNow.AddHours(24);

    //    user.EmailConfirmationCode = hash;
    //    user.EmailConfirmationTokenExpiresUtc = expires;

    //    repo.UpdateUser(user, ct);

    //    var url = $"{request.CallbackUrl}?userId={request.UserId}&token={token}";
    //    var body = $"""
    //    <p>Confirm your email by clicking the link below:</p>
    //    <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Confirm my email</a><p>
    //    <p>This link expires in 24 hours.</p>
    //    """;

    //    await email.SendEmailAsync(request.Email, "Confirm your email", body, ct);
    //}
}