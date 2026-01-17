using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{
    public record class RemoveUserCommand(string Id) : IRequest<RemoveUserResult>;

    public record class RemoveUserResult : Result;

    public class RemoveUserHandler : IRequestHandler<RemoveUserCommand, RemoveUserResult>
    {
        private readonly UserManager<User> _userManager;
        public RemoveUserHandler(UserManager<User> userManager)
        {
            _userManager = userManager;

        }
        public async Task<RemoveUserResult> Handle(RemoveUserCommand request, CancellationToken ct)
        {
            var validator = new RemoveUserValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var error = string.Join(";", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                return new() { ErrorMessage = error };
            }

            return new();
        }
    }
    public class RemoveUserValidator : AbstractValidator<RemoveUserCommand>
    {
        public RemoveUserValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id użytkownika nie może być puste.");
        }
    }
}