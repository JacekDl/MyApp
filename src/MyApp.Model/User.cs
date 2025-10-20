using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Model;

public class User : IdentityUser
{
    [Required, MaxLength(32)]
    public string Role { get; set; } = "User";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginDateTime { get; set; }

    [MaxLength(32)]
    public string? DisplayName { get; set; }

    public List<Review>? Reviews { get; set; }

    public static User Create(string email, string role = "User")
    {
        return new User
        {
            UserName = email,
            Email = email,
            Role = role
        };
    }
}