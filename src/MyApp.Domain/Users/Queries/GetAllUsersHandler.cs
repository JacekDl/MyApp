using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Model;


namespace MyApp.Domain.Users.Queries;

public record class GetAllUsersQuery() : IRequest<GetAllUsersResult>;

public record class GetAllUsersResult : Result<IReadOnlyList<UserDto>>; 

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, GetAllUsersResult>
{
    private readonly UserManager<User> _userManager;

    public GetAllUsersHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<GetAllUsersResult> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var result =  await _userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedUtc)
            .Select(u => new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.Role,
                u.DisplayName ?? string.Empty,
                u.CreatedUtc))
            .ToListAsync(ct);

        return new() { Value = result };
    }
}
