using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{

    public record class UpdateUserDetailsCommand(string Id, string? Name) : IRequest<UpdateUserDetailsResult>;

    public record class UpdateUserDetailsResult : Result<User>;


    public class UpdateUserDetailsHandler : IRequestHandler<UpdateUserDetailsCommand, UpdateUserDetailsResult>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUserDetailsHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<UpdateUserDetailsResult> Handle(UpdateUserDetailsCommand request, CancellationToken ct)
        {
            var validator = new UpdateUserDetailsValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                user.DisplayName = request.Name.Trim();
            }
            else
            {
                user.DisplayName = null;
            }

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                var message = string.Join(";", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
                return new() { ErrorMessage = message };
            }

            return new() { Value = user };
        }
    }

    public class UpdateUserDetailsValidator : AbstractValidator<UpdateUserDetailsCommand>
    {
        public UpdateUserDetailsValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");

            RuleFor(x => x.Name)
                .MaximumLength(User.DisplayNameMaxLength)
                    .WithMessage($"Nazwa wyświetlana nie może być dłuższa niż {User.DisplayNameMaxLength} znaków.")
                .When(x => x.Name is not null);
        }
    }
}