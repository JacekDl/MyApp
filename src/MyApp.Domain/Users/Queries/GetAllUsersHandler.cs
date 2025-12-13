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
        var users =  await _userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedUtc)
            .ToListAsync(ct);

        var list = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? string.Empty;

            list.Add(new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                primaryRole,
                user.DisplayName ?? string.Empty,
                user.CreatedUtc
            ));
        }

        return new() { Value = list };
    }
}
