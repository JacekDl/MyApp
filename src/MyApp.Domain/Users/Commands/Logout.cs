using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands;

public record LogoutCommand : IRequest;

public class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly SignInManager<User> _signInManager;

    public LogoutHandler(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        await _signInManager.SignOutAsync();
    }

}
