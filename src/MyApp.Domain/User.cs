using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace MyApp.Domain;

public class User
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Role { get; set; } = "User";

    [MaxLength(16)]
    public string? Name { get; set; }

    [MaxLength(32)]
    public string? PharmacyName { get; set; }

    [MaxLength(32)]
    public string? PharmacyCity { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public static User Create(string email, string role = "User")
    {
        return new User
        {
            Email = email,
            Role = role
        };
    }


}