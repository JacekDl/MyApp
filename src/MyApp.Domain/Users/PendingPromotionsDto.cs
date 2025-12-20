namespace MyApp.Domain.Users
{
    public record PendingPromotionsDto
    (
        int Id,
        string UserId,
        string Email,
        string? DisplayName,
        string FirstName,
        string LastName,
        string NumerPWZF,
        string Status,
        DateTime CreatedUtc
    );
}
