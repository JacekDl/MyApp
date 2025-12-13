using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Queries
{
    public record UserExistsByEmailQuery(string Email) : IRequest<UserExistsByEmailResult>;

    public record UserExistsByEmailResult : Result;

    public class UserExistsByEmailHandler : IRequestHandler<UserExistsByEmailQuery, UserExistsByEmailResult>
    {
        private readonly UserManager<User> _userManager;

        public UserExistsByEmailHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        public async Task<UserExistsByEmailResult> Handle (UserExistsByEmailQuery request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return new UserExistsByEmailResult{ ErrorMessage = "Użytkownik o podanym adresie email nie istnieje."};
            }
            return new();
        }
    }
}
