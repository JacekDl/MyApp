using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Common;
using MyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Domain.Users.Commands
{
    public record SendPasswordResetEmailCommand(string Email, string CallbackUrl, string? ReturnUrl) : IRequest<SendPasswordResetEmailResult>;

    public record SendPasswordResetEmailResult : Result;

    public class SendPasswordResetEmailHandler : IRequestHandler<SendPasswordResetEmailCommand, SendPasswordResetEmailResult>
    {
        private readonly IEmailSender _emailService;
        private readonly UserManager<User> _userManager;

        public SendPasswordResetEmailHandler(IEmailSender emailService, UserManager<User> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }



        public async Task<SendPasswordResetEmailResult> Handle(SendPasswordResetEmailCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return new();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = $"{request.CallbackUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            if(!string.IsNullOrWhiteSpace(request.ReturnUrl))
            {
                url += $"&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";
            }

            var body = $"""
                <p>Otrzymaliśmy prośbę o reset hasła.</p>
                <p>Kliknij w link, aby ustawić nowe hasło:</p>
                <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Resetuj hasło</a><p>
                <p>Jeśli to nie Ty prosiłeś o reset hasła, zignoruj tę wiadomość.</p>
                """;
            await _emailService.SendEmailAsync(user.Email!, "Reset hasła", body, ct);

            return new();
        }
    }
}
