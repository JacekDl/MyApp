using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.Web.ViewModels;
using System.Security.Claims;

namespace MyApp.Web.Controllers;

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

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Email), result.ErrorMessage!);
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
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
        if (!result.Succeeded)
        {
            return View("ConfirmEmailFailed");
        }

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

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Email), result.ErrorMessage!);
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

        if(!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Nie udało się zalogować.");
            return View(vm);
        }

        var user = result.Value!;
        var role = user.Role;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Users", "Admin");
        }

        if (role.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Reviews", "Pharmacist");
        }

        return RedirectToAction("Tokens", "Patient");
        
    }

    #endregion

    #region LogoutUser

    [HttpGet]
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
       
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View();
        }

        var vm = new DetailsViewModel();
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Szczegóły konta||"]);
        }
        else if (role == "Pharmacist")
        {
            vm.Breadcrumbs.AddRange(["Start|Reviews|Pharmacist", "Szczegóły konta||"]);
        }
        else
        {
            vm.Breadcrumbs.AddRange(["Start|Tokens|Patient", "Szczegóły konta||"]);
        }
        vm.User = result.Value!;

        return View(vm);
    }
    #endregion

    #region EditUserProfile
    [Authorize, HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View();
        }

        var vm = new EditProfileViewModel();
        vm.DisplayName = result.Value!.DisplayName;
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Szczegóły konta|Details|Account", "Zmiana imienia||"]);
        }
        else if (role == "Pharmacist")
        {
            vm.Breadcrumbs.AddRange(["Start|Reviews|Pharmacist", "Szczegóły konta|Details|Account", "Zmiana imienia||"]);
        }
        else
        {
            vm.Breadcrumbs.AddRange(["Start|Tokens|Patient", "Szczegóły konta|Details|Account", "Zmiana imienia||"]);
        }
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

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(vm);
        }

        TempData["Info"] = "Dane konta zostały zmienione. ";
        return RedirectToAction(nameof(Details));
    }
    #endregion

    #region ChangeUserEmail
    [Authorize, HttpGet]
    public async Task<IActionResult> ChangeEmail()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View();
        }

        var user = result.Value!;

        var vm = new ChangeEmailViewModel { Email = user.Email };
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Szczegóły konta|Details|Account", "Zmiana email||"]);
        }
        else if (role == "Pharmacist")
        {
            vm.Breadcrumbs.AddRange(["Start|Reviews|Pharmacist", "Szczegóły konta|Details|Account", "Zmiana email||"]);
        }
        else
        {
            vm.Breadcrumbs.AddRange(["Start|Tokens|Patient", "Szczegóły konta|Details|Account", "Zmiana email||"]);
        }
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
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
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
        var vm = new ChangePasswordViewModel();
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Szczegóły konta|Details|Account", "Zmiana hasła||"]);
        }
        else if (role == "Pharmacist")
        {
            vm.Breadcrumbs.AddRange(["Start|Reviews|Pharmacist", "Szczegóły konta|Details|Account", "Zmiana hasła||"]);
        }
        else
        {
            vm.Breadcrumbs.AddRange(["Start|Tokens|Patient", "Szczegóły konta|Details|Account", "Zmiana hasła||"]);
        }
        return View(vm);
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

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(vm);
        }

        TempData["Info"] = "Password changed.";
        return RedirectToAction(nameof(Details));
    }
    #endregion
}