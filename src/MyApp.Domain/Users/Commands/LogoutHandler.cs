using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record LogoutCommand() : IRequest<LogoutResult>;

public record LogoutResult : Result;

public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly SignInManager<User> _signInManager;

    public LogoutHandler(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken ct)
    {
        await _signInManager.SignOutAsync();
        return new();
    }

}
