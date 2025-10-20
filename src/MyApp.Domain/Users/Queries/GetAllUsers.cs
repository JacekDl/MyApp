using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Abstractions;
using MyApp.Model;


namespace MyApp.Domain.Users.Queries;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetAllUsersHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        return await _userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedUtc)
            .Select(u => new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.Role,
                u.DisplayName ?? string.Empty,
                u.CreatedUtc))
            .ToListAsync(ct);
    }
}
