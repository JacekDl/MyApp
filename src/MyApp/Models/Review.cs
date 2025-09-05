using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class Review
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    [Required, MaxLength(128)]
    public string Number { get; set; } = default!;

    [Required]
    public string Advice { get; set; } = default!;

    public string? ReviewText { get; set; }

    public bool Completed { get; set; }

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
