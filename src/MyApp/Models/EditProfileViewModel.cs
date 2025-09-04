using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class EditProfileViewModel
{
    [MaxLength(16)]
    public string? Name { get; set; }

    [MaxLength(32)]
    public string? PharmacyName { get; set; }

    [MaxLength(32)]
    public string? PharmacyCity { get; set; }
}
