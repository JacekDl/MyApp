using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record UpdateUserPasswordCommand(string Id, string CurrentPassword, string NewPassword) : IRequest<Result<User>>;

public class UpdateUserPasswordHandler : IRequestHandler<UpdateUserPasswordCommand, Result<User>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;

    public UpdateUserPasswordHandler(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Result<User>> Handle(UpdateUserPasswordCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result<User>.Fail("User not found");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            return Result<User>.Fail("Wrong password");

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        _repo.UpdateUser(user, ct);
        return Result<User>.Ok(user);
    }
}