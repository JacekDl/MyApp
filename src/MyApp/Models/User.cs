using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class User
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Role { get; set; } = "User";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}