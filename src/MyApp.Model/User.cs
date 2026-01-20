using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Model;

public class User : IdentityUser
{
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(32)]
    public string? DisplayName { get; set; }

    #region Constanst
    public const int PasswordMinLength = 6;
    public const int PasswordMaxLength = 128;
    public const int EmailMaxLength = 256;
    public const int DisplayNameMaxLength = 32;
    #endregion
}