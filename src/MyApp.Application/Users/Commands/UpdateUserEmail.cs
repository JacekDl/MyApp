using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Common;
using MyApp.Domain;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Application.Users.Commands;

public record UpdateUserEmailCommand(string Id, string Email, string Password) : IRequest<Result<User>>;

public class UpdateUserEmailHandler : IRequestHandler<UpdateUserEmailCommand, Result<User>>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UpdateUserEmailHandler(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<User>> Handle(UpdateUserEmailCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<User>.Fail("User not found");

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(request.Email))
            return Result<User>.Fail("Please enter a valid email address.");
        
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Result<User>.Fail("Wrong password");

        var newEmail = request.Email.Trim();
        var existingWithEmail = await _userManager.FindByEmailAsync(newEmail);
        if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
            return Result<User>.Fail("Email is already registered.");

        var setEmail = await _userManager.SetEmailAsync(user, newEmail);
        if (!setEmail.Succeeded)
        {
            var message = string.Join("; ", setEmail.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result<User>.Fail(message);
        }
        
        user.UserName = newEmail;
        user.EmailConfirmed = false;
        var update = await _userManager.UpdateAsync(user);
        if(!update.Succeeded)
        {
            var message = string.Join("; ", update.Errors.Select(e => $"{e.Code}: {e.Description}"));
        }

        await _signInManager.RefreshSignInAsync(user);
        return Result<User>.Ok(user);
    }
}
