using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record SendEmailConfirmationCommand(string UserId, string CallbackUrl) : IRequest;

public class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly IEmailSender _emailService;
    private readonly UserManager<User> _userManager;

    public SendEmailConfirmationHandler(IEmailSender emailService, UserManager<User> userManager)
    {
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user is null)
            throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (user.EmailConfirmed)
            return;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var url = $"{request.CallbackUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        var body = $"""
        <p>Potwierdź swój email klikając poniższy link:</p>
        <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Potwierdź swój email</a><p>
        <p>Link jest ważny przez 24 godziny.</p>
        """;

        await _emailService.SendEmailAsync(user.Email!, "Potwierdź email", body, ct);
    }
}