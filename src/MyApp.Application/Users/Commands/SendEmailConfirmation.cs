using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record SendEmailConfirmationCommand(int UserId, string Email, string CallbackUrl) : IRequest;

public class SendEmailConfirmationHandler(IUserRepository repo, IEmailSender email) : IRequestHandler<SendEmailConfirmationCommand>
{
    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new KeyNotFoundException($"User {request.UserId} not found.");
        }

        if (user.EmailConfirmed)
            return;

        var token = EmailToken.CreateToken();
        var hash = EmailToken.Hash(token);
        var expires = DateTimeOffset.UtcNow.AddHours(24);

        user.EmailConfirmationCode = hash;
        user.EmailConfirmationTokenExpiresUtc = expires;

        repo.UpdateUser(user, ct);

        var url = $"{request.CallbackUrl}?userId={request.UserId}&token={token}";
        var body = $"""
        <p>Confirm your email by clicking the link below:</p>
        <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Confirm my email</a><p>
        <p>This link expires in 24 hours.</p>
        """;

        await email.SendEmailAsync(request.Email, "Confirm your email", body, ct);
    }
}