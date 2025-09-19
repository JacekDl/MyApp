using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels;

public class EditProfileViewModel
{
    [MaxLength(16)]
    public string? DisplayName { get; set; }

    [MaxLength(32)]
    public string? PharmacyName { get; set; }

    [MaxLength(32)]
    public string? PharmacyCity { get; set; }
}
