using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;

namespace MyApp.Application.Users.Commands;

public record RemoveUserCommand(string Id) : IRequest<Result<bool>>;

public class RemoveUserHandler : IRequestHandler<RemoveUserCommand, Result<bool>>
{
    private readonly IUserRepository _repo;
    public RemoveUserHandler(IUserRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<bool>> Handle(RemoveUserCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result<bool>.Fail("User not found");

        _repo.RemoveAsync(user, ct);
        return Result<bool>.Ok(true);
    }
}