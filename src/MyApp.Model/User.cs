using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Model;

public class User : IdentityUser
{
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(32)]
    public string? DisplayName { get; set; }
}