namespace MyApp.Domain.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
