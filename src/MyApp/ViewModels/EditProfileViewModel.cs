using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels;

public class EditProfileViewModel
{
    [MaxLength(16)]
    public string? DisplayName { get; set; }

}
