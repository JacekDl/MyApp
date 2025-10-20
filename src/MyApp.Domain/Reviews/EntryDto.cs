namespace MyApp.Domain.Reviews;

public record EntryDto(
    string? UserId, 
    string Text, 
    DateTime CreatedUtc,
    string DisplayName
    );
