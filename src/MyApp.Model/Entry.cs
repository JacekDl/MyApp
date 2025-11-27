using System.ComponentModel.DataAnnotations;

namespace MyApp.Model;

public class Entry
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Wiadomość nie może być pusta.")]
    public string Text { get; set; } = default!;

    public string? UserId { get; set; }
    public User? User { get; set; }

    public int ReviewId { get; set; }
    public Review Review { get; set; } = default!;

    public static Entry Create(string userId, string text) =>
        new Entry { UserId = userId, Text = text.Trim() };

    public static Entry CreateAnonymous(string text)
        => new() { UserId = null, Text = text.Trim() };
}