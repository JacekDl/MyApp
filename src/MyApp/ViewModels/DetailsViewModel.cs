using MyApp.Domain.Users;

namespace MyApp.Web.ViewModels;

public class DetailsViewModel : ViewModelBase
{
    public UserDto User { get; set; } = null!;
}
