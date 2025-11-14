using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Model;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Domain.Users.Commands;

public record class UpdateUserEmailCommand(string Id, string Email, string Password) : IRequest<UpdateUserEmailResult>;

public record class UpdateUserEmailResult : Result<User>;

public class UpdateUserEmailHandler : IRequestHandler<UpdateUserEmailCommand, UpdateUserEmailResult>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UpdateUserEmailHandler(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<UpdateUserEmailResult> Handle(UpdateUserEmailCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new() {ErrorMessage = "Nie znaleziono użytkownika."};
        }

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(request.Email))
        {
            return new() { ErrorMessage = "Proszę wprowadzić prawidłowy adres e-mail." };
        }
        
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new() { ErrorMessage = "Nieprawidłowe hasło." };
        }

        var newEmail = request.Email.Trim();
        var existingWithEmail = await _userManager.FindByEmailAsync(newEmail);
        if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
        {
            return new() { ErrorMessage = "Podany email jest już zarejestrowany." };
        }

        var setEmail = await _userManager.SetEmailAsync(user, newEmail);
        if (!setEmail.Succeeded)
        {
            var message = string.Join("; ", setEmail.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return new() { ErrorMessage = message };
        }
        
        user.UserName = newEmail;
        user.EmailConfirmed = false;
        var update = await _userManager.UpdateAsync(user);
        if(!update.Succeeded)
        {
            var message = string.Join("; ", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
        }

        await _signInManager.RefreshSignInAsync(user);
        return new() { Value = user };
    }
}
