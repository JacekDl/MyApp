using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;


namespace MyApp.Application.Users.Queries;

public record GetUserByIdQuery(string Id) : IRequest<Result<UserDto>>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _repo;

    public GetUserByIdHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result<UserDto>.Fail("User not found");

        var userDto = new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.Role,
            user.UserName!,
            user.PharmacyName!,
            user.PharmacyCity!,
            user.CreatedUtc);
        return Result<UserDto>.Ok(userDto);
    }
}
