using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Model;


namespace MyApp.Domain.Users.Queries;

public record class GetAllUsersQuery(int Page = 1, int PageSize = 10) : IRequest<GetAllUsersResult>;

public record class GetAllUsersResult : PagedResult<List<UserDto>>; 

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, GetAllUsersResult>
{
    private readonly UserManager<User> _userManager;

    public GetAllUsersHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<GetAllUsersResult> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var query = _userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedUtc);

        var totalCount = await query.CountAsync(cancellationToken: ct);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new() { Value = list, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}
