namespace MyApp.Application.Users;

public record UserDto(
    int Id,
    string Email,
    string Role,
    string Name,
    string PharmacyName,
    string PharmacyCity,
    DateTime CreatedUtc
    );
