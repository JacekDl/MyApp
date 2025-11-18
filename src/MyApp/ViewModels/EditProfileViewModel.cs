using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels;

public class EditProfileViewModel : ViewModelBase
{
    [MaxLength(16)]
    public string? DisplayName { get; set; }

}
