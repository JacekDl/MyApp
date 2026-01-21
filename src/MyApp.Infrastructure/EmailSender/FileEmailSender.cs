using Microsoft.Extensions.Hosting;
using MyApp.Domain.Abstractions;

namespace MyApp.Infrastructure;

public class FileEmailSender : IEmailSender
{
    private readonly string _root;
    public FileEmailSender(IHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "App_Data", "Email");
        Directory.CreateDirectory(_root);
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var file = Path.Combine(_root, $"{DateTime.UtcNow:yyyyMMdd_HHmmssffff}_{Guid.NewGuid():N}.html");
        var content = $"""
        <h3>To: {System.Net.WebUtility.HtmlEncode(to)}</h3>
        <h4>Subject: {System.Net.WebUtility.HtmlEncode(subject)}</h4>
        <hr/>
        {htmlBody}
        """;
        return File.WriteAllTextAsync(file, content, ct);
    }
}
