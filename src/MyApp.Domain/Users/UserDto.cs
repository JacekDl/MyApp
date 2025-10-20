namespace MyApp.Domain.Users;

public record UserDto(
    string Id,
    string Email,
    string Role,
    string DisplayName,
    DateTime CreatedUtc
    );
