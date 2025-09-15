using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Application.Users.Commands;

public record CreateUserCommand(string Email, string Password) : IRequest<Result<User>>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<User>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;
    public CreateUserHandler(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var normalized = request.Email.Trim().ToLower();

        if (await _repo.GetByEmailAsync(normalized, ct) is not null)
        {
            return Result<User>.Fail("Email is already registered.");
        }
        var user = User.Create(request.Email);
        var passwordHash = _hasher.HashPassword(user, request.Password);
        user.PasswordHash = passwordHash;

        _repo.CreateUser(user, ct);
        return Result<User>.Ok(user);
    }
}
