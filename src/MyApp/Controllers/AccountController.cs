using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Services;
using System.Security.Claims;

namespace MyApp.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _users;

    public AccountController(IUserService users)
    {
        _users = users;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _users.RegisterAsync(model.Email, model.Password, role: "User");

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Email), result.Error!);
            return View(model);
        }

        await SignInAsync(result.Value!, isPersistent: true); 
        return RedirectAfterSignIn(result.Value!);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _users.ValidateCredentialsAsync(model.Email, model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectAfterSignIn(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize,HttpGet]
    public async Task<IActionResult> Details()
    {
        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        ViewBag.Email = user.Email;
        ViewBag.Name = user.Name;
        ViewBag.PharmacyName = user.PharmacyName;
        ViewBag.PharmacyCity = user.PharmacyCity;
        ViewBag.CreatedUtc = user.CreatedUtc;
        return View();
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        return View (new EditProfileViewModel
        {
            Name = user.Name,
            PharmacyName = user.PharmacyName,
            PharmacyCity = user.PharmacyCity
        });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _users.UpdateProfileAsync(CurrentUserId, model.Name, model.PharmacyName, model.PharmacyCity);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Info"] = "Profile updated.";
        return RedirectToAction(nameof(Details));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> ChangeEmail()
    {
        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(CurrentUserId);
        if (user is null)
        {
            return NotFound();
        }

        var model = new ChangeEmailViewModel { Email = user.Email };
        return View(model);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _users.UpdateEmailAsync(CurrentUserId, model.Email, model.CurrentPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Info"] = "Email updated.";
        return RedirectToAction(nameof(Details));
    }

    [AllowAnonymous]
    public IActionResult Denied() => Content("Access Denied");

    private RedirectToActionResult RedirectAfterSignIn(User user)
    {
        // Simple role-based redirect (adjust as needed)
        if (User.IsInRole("Admin") || string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Admin");

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
}
