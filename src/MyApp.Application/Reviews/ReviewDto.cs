namespace MyApp.Application.Reviews;

public record ReviewDto
(
    int Id,
    int CreatedByUserId,
    DateTime DateCreated,
    string Advice,
    string ReviewText,
    bool Completed
);
