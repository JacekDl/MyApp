using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record GetUserCommand(string Email, string Password) : IRequest<Result<User>>;

public class GetUserHandler : IRequestHandler<GetUserCommand, Result<User>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;
    public GetUserHandler(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Result<User>> Handle(GetUserCommand request, CancellationToken ct)
    {
        var normalized = request.Email.Trim().ToLower();
        var user = await _repo.GetByEmailAsync(normalized, ct);
        if (user is null)
            return Result<User>.Fail("Wrong email or password");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<User>.Fail("Wrong email or password");
        else
            return Result<User>.Ok(user);
    }
}
