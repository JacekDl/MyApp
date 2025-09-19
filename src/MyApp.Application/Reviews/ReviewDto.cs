namespace MyApp.Application.Reviews;

public record ReviewDto
(
    int Id,
    string CreatedByUserId,
    string Number,
    DateTime DateCreated,
    string Advice,
    string ReviewText,
    bool Completed
);
