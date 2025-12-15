using MyApp.Domain.Users;

namespace MyApp.Web.ViewModels;

public class UsersViewModel : PagedViewModel
{
    public List<UserDto> Users { get; set; } = [];
}

