using MediatR;
using MyApp.Application.Abstractions;


namespace MyApp.Application.Users.Queries;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _repo;

    public GetAllUsersHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(ct);
        return list
            .Select(u => new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.Role,
                u.UserName!,
                u.PharmacyName!,
                u.PharmacyCity!,
                u.CreatedUtc))
            .OrderByDescending(u => u.CreatedUtc)
            .ToList();
    }
}
