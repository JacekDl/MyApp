using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.ViewModels;
using System.Security.Claims;

namespace MyApp.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region RegisterPharmacist
    [HttpGet, AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var vm = new RegisterViewModel { PostAction = nameof(Register) };
        return View(vm);
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password, "Pharmacist"));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(nameof(vm.Email), result.Error!);
            return View(vm);
        }

        var user = result.Value!;
        var callbackBase = Url.Action(nameof(ConfirmEmail), "Account", null, Request.Scheme)!;
        await _mediator.Send(new SendEmailConfirmationCommand(user.Id, callbackBase));

        return RedirectToAction(nameof(ConfirmEmailSent));
    }

    [AllowAnonymous]
    public IActionResult ConfirmEmailSent()
    { 
        return View(); 
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(token)) return BadRequest();

        var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
        if (!result) return View("ConfirmEmailFailed");

        if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return View("EmailConfirmed");
    }
    #endregion

    #region RegisterPatient
    [HttpGet, AllowAnonymous]
    public IActionResult RegisterPatient(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var vm = new RegisterViewModel { PostAction = nameof(RegisterPatient) };
        return View("Register", vm);
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPatient(RegisterViewModel vm)
    {
        
        if (!ModelState.IsValid)
        {
            return View("Register", vm);
        }

        var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password, "Patient"));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(nameof(vm.Email), result.Error!);
            return View(vm);
        }

        var user = result.Value!;
        var callbackBase = Url.Action(nameof(ConfirmEmail), "Account", null, Request.Scheme)!;
        await _mediator.Send(new SendEmailConfirmationCommand(user.Id, callbackBase));

        return RedirectToAction(nameof(ConfirmEmailSent));
    }

    #endregion

    #region LoginUser
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _mediator.Send(new LoginCommand(vm.Email, vm.Password, vm.RememberMe));

        switch(result.Status)
        {
            case LoginStatus.Succeeded:
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                    
                if (string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("ViewUsers", "Admin");
                }
                
                if (string.Equals(result.Role, "Pharmacist", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Reviews", "Pharmacist");
                }

                return RedirectToAction("Tokens", "Patient");
                

            case LoginStatus.NotAllowed:
            case LoginStatus.LockedOut:
            case LoginStatus.WrongCredentials:
                ModelState.AddModelError(string.Empty, result.Message ?? "Login failed.");
                return View(vm);
        }

        ModelState.AddModelError(string.Empty, "Login failed.");
        return View(vm);
    }

    #endregion

    #region LogoutUser

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region UserDetails
    [Authorize,HttpGet]
    public async Task<IActionResult> Details()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View();
        }

        var dto = result.Value!;
        return View(dto);
    }
    #endregion

    #region EditUserProfile
    [Authorize, HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View();
        }

        var vm = new EditProfileViewModel
        {
            DisplayName = result.Value!.DisplayName,
        };
        return View(vm);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UpdateUserDetailsCommand(currentUserId, vm.DisplayName));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        TempData["Info"] = "Profile updated.";
        return RedirectToAction(nameof(Details));
    }
    #endregion

    #region ChangeUserEmail
    [Authorize, HttpGet]
    public async Task<IActionResult> ChangeEmail()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View();
        }

        var user = result.Value!;

        var vm = new ChangeEmailViewModel { Email = user.Email };
        return View(vm);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new UpdateUserEmailCommand(currentUserId, vm.Email, vm.CurrentPassword));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        var callbackBase = Url.Action(nameof(ConfirmEmail), "Account", null, Request.Scheme)!;
        await _mediator.Send(new SendEmailConfirmationCommand(currentUserId, callbackBase));

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return RedirectToAction(nameof(ConfirmEmailSent));
    }
    #endregion

    #region ChangeUserPassword
    [Authorize, HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UpdateUserPasswordCommand(CurrentUserId, vm.CurrentPassword, vm.NewPassword));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        TempData["Info"] = "Password changed.";
        return RedirectToAction(nameof(Details));
    }
    #endregion
}