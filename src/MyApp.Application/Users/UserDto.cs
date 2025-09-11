namespace MyApp.Application.Users;

public record UserDto(
    int Id,
    string Email,
    string Role,
    DateTime CreatedUtc);
