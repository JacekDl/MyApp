using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{
    public record SendEmailConfirmationCommand(string UserId, string CallbackUrl) : IRequest<SendEmailConfirmationResult>;

    public record class SendEmailConfirmationResult : Result;

    public class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand, SendEmailConfirmationResult>
    {
        private readonly IEmailSender _emailService;
        private readonly UserManager<User> _userManager;

        public SendEmailConfirmationHandler(IEmailSender emailService, UserManager<User> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<SendEmailConfirmationResult> Handle(SendEmailConfirmationCommand request, CancellationToken ct)
        {
            var validator = new SendEmailConfirmationValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono Id użytkownika." };
            }

            if (user.EmailConfirmed)
            {
                return new();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var url = $"{request.CallbackUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var body = $"""
        <p>Potwierdź swój email klikając poniższy link:</p>
        <p><a href="{System.Net.WebUtility.HtmlEncode(url)}">Potwierdź swój email</a><p>
        <p>Link jest ważny przez 24 godziny.</p>
        """;

            await _emailService.SendEmailAsync(user.Email!, "Potwierdź email", body, ct);

            return new();
        }
    }

    public class SendEmailConfirmationValidator : AbstractValidator<SendEmailConfirmationCommand>
    {
        public SendEmailConfirmationValidator()
        {
            RuleFor(x => x.UserId)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");

            RuleFor(x => x.CallbackUrl)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                    .WithMessage("Adres callback nie może być pusty.");
        }
    }
}