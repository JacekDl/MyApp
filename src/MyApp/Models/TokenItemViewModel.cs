namespace MyApp.Models;

public sealed class TokenItemViewModel
{
    public int Id { get; init; }
    public string Number { get; init; }
    public string Advice { get; init; }
    public string ReviewText { get; init; } = string.Empty;
    public DateTime DateCreated { get; init; }
    public bool Completed { get; init; }
}
