using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Application.Users.Commands;

public record UpdateUserEmailCommand(string Id, string Email, string Password) : IRequest<Result<User>>;

public class UpdateUserEmailHandler : IRequestHandler<UpdateUserEmailCommand, Result<User>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;

    public UpdateUserEmailHandler(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Result<User>> Handle(UpdateUserEmailCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result<User>.Fail("User not found");

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(request.Email))
            return Result<User>.Fail("Please enter a valid email address.");
        
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<User>.Fail("Wrong password");

        var normalized = request.Email.Trim().ToLower();
        if (await _repo.GetByEmailAsync(normalized, ct) is not null)
            return Result<User>.Fail("Email is already registered.");
        
        user.Email = request.Email.Trim();
        _repo.UpdateUser(user, ct);
        return Result<User>.Ok(user);
    }
}
