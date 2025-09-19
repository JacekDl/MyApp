namespace MyApp.Application.Users;

public record UserDto(
    string Id,
    string Email,
    string Role,
    string DisplayName,
    string PharmacyName,
    string PharmacyCity,
    DateTime CreatedUtc
    );
