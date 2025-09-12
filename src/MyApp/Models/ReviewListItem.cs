namespace MyApp.Models;

public class ReviewListItem
{
    public int Id { get; init; }

    public int CreatedByUserId { get; init; }
    public DateTime DateCreated { get; init; }
    public string Advice { get; init; } = string.Empty;
    public string ReviewText { get; init; } = string.Empty;
    public bool Completed { get; init; }
}
