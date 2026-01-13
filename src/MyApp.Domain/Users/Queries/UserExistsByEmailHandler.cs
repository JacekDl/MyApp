using FluentValidation;
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
            var validator = new UserExistsByEmailValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return new UserExistsByEmailResult{ ErrorMessage = "Użytkownik o podanym adresie email nie istnieje."};
            }
            return new();
        }
    }

    public class UserExistsByEmailValidator : AbstractValidator<UserExistsByEmailQuery>
    {
        public UserExistsByEmailValidator()
        {
            RuleFor(x => x.Email)
                .Must(e => !string.IsNullOrWhiteSpace(e))
                    .WithMessage("Adres e-mail nie może być pusty.")
                .EmailAddress()
                    .WithMessage("Nieprawidłowy adres e-mail.")
                .MaximumLength(256)
                    .WithMessage("Adres e-mail nie może być dłuższy niż 256 znaków.");
        }
    }
}
