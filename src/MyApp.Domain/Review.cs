using System.ComponentModel.DataAnnotations;

namespace MyApp.Domain;

public class Review
{
    public int Id { get; set; }

    public string CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; }
    public DateTime DateCreated { get; set; }

    [Required, MaxLength(128)]
    public string Number { get; set; } = default!;

    // To create Review pharmacist must include some advice.
    [Required]
    public string Advice { get; set; } = default!;

    // Response is initially empty.
    public string? Response { get; set; }

    public bool Completed { get; set; }




    public static Review Create(string userId, string advice, string number)
    {
        return new Review
        {
            CreatedByUserId = userId,
            Number = number,
            Advice = advice.Trim(),
            DateCreated = DateTime.UtcNow,
            Completed = false
        };
    }
}
