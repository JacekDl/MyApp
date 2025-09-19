namespace MyApp.Application.Users;

public record UserDto(
    string Id,
    string Email,
    string Role,
    string UserName,
    string PharmacyName,
    string PharmacyCity,
    DateTime CreatedUtc
    );
