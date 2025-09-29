using System.ComponentModel.DataAnnotations;

namespace MyApp.Domain;

public class Entry
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;

    public int ReviewId { get; set; }
    public Review Review { get; set; } = default!;

    public static Entry Create(string userId, string text) =>
        new Entry { UserId = userId, Text = text.Trim() };
}