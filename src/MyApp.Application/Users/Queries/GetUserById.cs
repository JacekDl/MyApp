using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Common;
using MyApp.Domain;


namespace MyApp.Application.Users.Queries;

public record GetUserByIdQuery(string Id) : IRequest<Result<UserDto>>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<UserDto>.Fail("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role;

        var userDto = new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.Role,
            user.DisplayName ?? string.Empty,
            user.PharmacyName ?? string.Empty,
            user.PharmacyCity ?? string.Empty,
            user.CreatedUtc
        );

        return Result<UserDto>.Ok(userDto);
    }
}
