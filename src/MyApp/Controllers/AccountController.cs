using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain;
using MyApp.Services;
using System.Security.Claims;
using MyApp.ViewModels;
using MediatR;
using MyApp.Application.Users.Commands;
using MyApp.Application.Users.Queries;
using System.Net.WebSockets;

namespace MyApp.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _users;
    private readonly IMediator _mediator;

    public AccountController(IUserService users, IMediator mediator)
    {
        _users = users;
        _mediator = mediator;
    }

    #region RegisterUser
    [HttpGet, AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(nameof(vm.Email), result.Error!);
            return View(vm);
        }

        await SignInAsync(result.Value!, isPersistent: false);
        return RedirectAfterSignIn(result.Value!);
    }
    #endregion

    private RedirectToActionResult RedirectAfterSignIn(User user)
    {
        if (User.IsInRole("Admin") || string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("ViewUsers", "Admin");

        return RedirectToAction("Reviews", "UserRole");
    }

    private async Task SignInAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

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

        var result = await _mediator.Send(new GetUserQuery(vm.Email, vm.Password));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        await SignInAsync(result.Value!, vm.RememberMe);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectAfterSignIn(result.Value!);
    }
    #endregion

    #region LogoutUser
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    #endregion

    #region UserDetails
    [Authorize,HttpGet]
    public async Task<IActionResult> Details()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new GetUserByIdQuery(currentUserId));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View();
        }

        var vm = new EditProfileViewModel
        {
            Name = result.Value!.Name,
            PharmacyName = result.Value.PharmacyName,
            PharmacyCity = result.Value.PharmacyCity
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

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateUserDetailsCommand(currentUserId, vm.Name, vm.PharmacyName, vm.PharmacyCity));

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
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new UpdateUserEmailCommand(currentUserId, vm.Email, vm.CurrentPassword));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        TempData["Info"] = "Email updated.";
        return RedirectToAction(nameof(Details));
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

        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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