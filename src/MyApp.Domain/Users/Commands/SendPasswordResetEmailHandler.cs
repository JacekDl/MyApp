using FluentValidation;
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
            var validator = new SendPasswordResetEmailValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByEmailAsync(request.Email.Trim());

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

    public class SendPasswordResetEmailValidator : AbstractValidator<SendPasswordResetEmailCommand>
    {
        public SendPasswordResetEmailValidator()
        {
            RuleFor(x => x.Email)
                .Must(e => !string.IsNullOrWhiteSpace(e))
                    .WithMessage("Adres e-mail nie może być pusty.")
                .EmailAddress()
                    .WithMessage("Nieprawidłowy adres e-mail.")
                .MaximumLength(256)
                    .WithMessage("Adres e-mail nie może być dłuższy niż 256 znaków.");

            RuleFor(x => x.CallbackUrl)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                    .WithMessage("Adres callback nie może być pusty.");
        }
    }
}
