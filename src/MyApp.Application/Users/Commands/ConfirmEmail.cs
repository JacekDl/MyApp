using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using System.Security.Cryptography;

namespace MyApp.Application.Users.Commands;

public record ConfirmEmailCommand(int UserId, string Token) : IRequest<bool>;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly IUserRepository _repo;

    public ConfirmEmailHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.UserId, ct);

        var tokenHash = EmailToken.Hash(request.Token);

        if (user is null)
        {
            return false;
        }

        if (user.EmailConfirmed)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.EmailConfirmationCode) ||
            user.EmailConfirmationTokenExpiresUtc is null)
            return false;

        if (DateTimeOffset.UtcNow > user.EmailConfirmationTokenExpiresUtc.Value)
            return false;

        if (!FixedTimeEqualsHex(user.EmailConfirmationCode, tokenHash))
            return false;

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationTokenExpiresUtc = null;

        _repo.UpdateUser(user, ct);

        return true;
    }

    private static bool FixedTimeEqualsHex(string aHex, string bHex)
    {
        try
        {
            var a = Convert.FromHexString(aHex);
            var b = Convert.FromHexString(bHex);
            if (a.Length != b.Length) return false;
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
        catch
        {
            return false;
        }
    }
}