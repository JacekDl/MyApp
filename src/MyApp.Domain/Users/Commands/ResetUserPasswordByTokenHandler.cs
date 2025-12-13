using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Domain.Users.Commands
{
    public record ResetUserPasswordByTokenCommand(string UserId, string Token, string NewPassword) : IRequest<ResetUserPasswordByTokenResult>;

    public record ResetUserPasswordByTokenResult : Result;


    public class ResetUserPasswordByTokenHandler : IRequestHandler<ResetUserPasswordByTokenCommand, ResetUserPasswordByTokenResult>
    {
        private readonly UserManager<User> _userManager;

        public ResetUserPasswordByTokenHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResetUserPasswordByTokenResult> Handle(ResetUserPasswordByTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            var identityResult = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                return new() { ErrorMessage = $"Nie udało się zresetować hasła: {errors}" };
            }

            return new();
        }
    }
}
