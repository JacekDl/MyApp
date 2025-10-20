namespace MyApp.Domain.Reviews;

public record ReviewDto
(
    int Id,
    string PharmacistId,
    string Number,
    DateTime DateCreated,
    string Text,
    string ReviewText, //not used
    bool Completed,
    bool IsNewForViewer
);
