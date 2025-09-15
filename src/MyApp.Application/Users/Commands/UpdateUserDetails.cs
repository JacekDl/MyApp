using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;

namespace MyApp.Application.Users.Commands;

public record UpdateUserDetailsCommand(int Id, string? Name, string? PharmacyName, string? PharmacyCity) : IRequest<Result<User>>;


public class UpdateUserDetailsHandler : IRequestHandler<UpdateUserDetailsCommand, Result<User>>
{
    private readonly IUserRepository _repo;
    public UpdateUserDetailsHandler(IUserRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<User>> Handle(UpdateUserDetailsCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result<User>.Fail("User not found");

        user.Name = request.Name?.Trim();
        user.PharmacyName = request.PharmacyName?.Trim();
        user.PharmacyCity = request.PharmacyCity?.Trim();
        _repo.UpdateUser(user, ct);
        return Result<User>.Ok(user);
    }
}