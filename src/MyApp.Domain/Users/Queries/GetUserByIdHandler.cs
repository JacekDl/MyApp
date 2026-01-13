using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Model;


namespace MyApp.Domain.Users.Queries
{

    public record class GetUserByIdQuery(string Id) : IRequest<GetUserByIdResult>;

    public record class GetUserByIdResult : Result<UserDto>;

    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResult>
    {
        private readonly UserManager<User> _userManager;

        public GetUserByIdHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GetUserByIdResult> Handle(GetUserByIdQuery request, CancellationToken ct)
        {
            var validator = new GetUserByIdValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? string.Empty;

            var userDto = new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                primaryRole,
                user.DisplayName ?? string.Empty,
                user.CreatedUtc
            );

            return new() { Value = userDto };
        }
    }

    public class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");
        }
    }
}
