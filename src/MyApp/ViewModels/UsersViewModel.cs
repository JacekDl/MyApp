using MyApp.Domain.Users;

namespace MyApp.Web.ViewModels;

public class UsersViewModel : ViewModelBase
{
    public List<UserDto> Users { get; set; } = [];
}

