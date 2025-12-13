using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.Model;
using MyApp.Web.ViewModels;
using MyApp.Web.ViewModels.Common;
using System.Security.Claims;

namespace MyApp.Web.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AccountController(
        IMediator mediator, 
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    #region Register
    [HttpGet, AllowAnonymous]
    public IActionResult Register(string role = "Patient", string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Role"] = role;

        var vm = new RegisterViewModel
        {
            PostAction = nameof(Register),
            // if you can: add Role to VM instead of ViewData
            // Role = role
        };

        return View(vm);
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm , string role = "Patient", string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Role"] = role;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        role = NormalizeRole(role);
        var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password, role));

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

    private static string NormalizeRole(string? role)
    {
        return role?.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase) == true
            ? "Pharmacist"
            : "Patient";
    }

    #endregion

    #region RegisterPharmacist
    //[HttpGet, AllowAnonymous]
    //public IActionResult Register(string? returnUrl = null)
    //{
    //    ViewData["ReturnUrl"] = returnUrl;
    //    var vm = new RegisterViewModel { PostAction = nameof(Register) };
    //    return View(vm);
    //}

    //[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    //public async Task<IActionResult> Register(RegisterViewModel vm)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return View(vm);
    //    }

    //    var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password, "Pharmacist"));

    //    if (!result.Succeeded)
    //    {
    //        ModelState.AddModelError(nameof(vm.Email), result.ErrorMessage!);
    //        return View(vm);
    //    }

    //    var user = result.Value!;
    //    var callbackBase = Url.Action(nameof(ConfirmEmail), "Account", null, Request.Scheme)!;
    //    await _mediator.Send(new SendEmailConfirmationCommand(user.Id, callbackBase));

    //    return RedirectToAction(nameof(ConfirmEmailSent));
    //}

    [AllowAnonymous]
    public IActionResult ConfirmEmailSent()
    {
        var vm = new InfoViewModel();
        vm.Message = "Na podany adres email został wysłany link potwierdzający. " +
            "Kliknij w niego, aby aktywować konto.";
        return View("Info", vm);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(); //TODO: zmienić
        }

        var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
        if (!result.Succeeded)
        {
            var error = new ErrorViewModel { Message = "Nie udało się potwierdzić adresu email." };
            return View("Error", error);
        }

        if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var info = new InfoViewModel { Message = "Potwierdziliśmy Twój adres email. Możesz teraz zalogować się na swoje konto." };
        return View("Info", info);
    }
    #endregion

    #region RegisterPatient
    //[HttpGet, AllowAnonymous]
    //public IActionResult RegisterPatient(string? returnUrl = null)
    //{
    //    ViewData["ReturnUrl"] = returnUrl;
    //    var vm = new RegisterViewModel { PostAction = nameof(RegisterPatient) };
    //    return View("Register", vm);
    //}

    //[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    //public async Task<IActionResult> RegisterPatient(RegisterViewModel vm)
    //{
        
    //    if (!ModelState.IsValid)
    //    {
    //        return View("Register", vm);
    //    }

    //    var result = await _mediator.Send(new CreateUserCommand(vm.Email, vm.Password, "Patient"));

    //    if (!result.Succeeded)
    //    {
    //        ModelState.AddModelError(nameof(vm.Email), result.ErrorMessage!);
    //        vm.PostAction = nameof(RegisterPatient);
    //        return View("Register", vm);
    //    }

    //    var user = result.Value!;
    //    var callbackBase = Url.Action(nameof(ConfirmEmail), "Account", null, Request.Scheme)!;
    //    await _mediator.Send(new SendEmailConfirmationCommand(user.Id, callbackBase));

    //    return RedirectToAction(nameof(ConfirmEmailSent));
    //}

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
        return RedirectAfterLogin(user, returnUrl);

    }
    #endregion

    #region ExternalLogin

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? role = null, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl, role });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        if(!string.IsNullOrEmpty(role))
        {
            properties.Items["role"] = role;

        }

        return Challenge(properties, provider);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null, string? role = null)
    {
        returnUrl ??= Url.Content("~/");
        role = string.IsNullOrWhiteSpace(role) ? null : NormalizeRole(role);

        if (remoteError != null)
        {
            TempData["Error"] = "Błąd logowania zewnętrznego: " + remoteError;
            return RedirectToAction(nameof(Login), new {returnUrl});
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["Error"] = "Nie udało się pobrać informacji o logowaniu zewnętrznym.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false);

        if (signInResult.Succeeded)
        {
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser != null)
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                var primaryRole = roles.FirstOrDefault() ?? string.Empty;

                var dto = new UserDto(
                    existingUser.Id,
                    existingUser.Email ?? string.Empty,
                    primaryRole,
                    existingUser.DisplayName ?? string.Empty,
                    existingUser.CreatedUtc);
                return RedirectAfterLogin(dto, returnUrl);
            }

            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Dostawca logowania nie udostępnił adresu email.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null && string.IsNullOrWhiteSpace(role))
        {
            TempData["Info"] = "Wybierz rolę, aby dokończyć rejestrację przez Google.";
            return RedirectToAction(nameof(Register), new { returnUrl });
        }

        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = name,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["Error"] = "Nie udało się utworzyć konta na podstawie danych Google.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            await _userManager.AddToRoleAsync(user, role);
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            TempData["Error"] = "Nie udało się powiązać logowania zewnętrznego z kontem.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        var userRoles = await _userManager.GetRolesAsync(user);
        var userPrimaryRole = userRoles.FirstOrDefault() ?? string.Empty;

        var userDto = new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            userPrimaryRole,
            user.DisplayName ?? string.Empty,
            user.CreatedUtc);
        return RedirectAfterLogin(userDto, returnUrl);
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

        await _signInManager.RefreshSignInAsync(result.Value!);

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

        await _signInManager.RefreshSignInAsync(result.Value!);

        TempData["Info"] = "Hasło zostało zmienione.";
        return RedirectToAction(nameof(Details));
    }
    #endregion

    #region DeleteAccount
    public async Task<IActionResult> DeleteProfile()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var logoutResult = await _mediator.Send(new LogoutCommand());
        if (!logoutResult.Succeeded)
        {
            TempData["Error"] = "Wystąpił błąd podczas wylogowywania.";
            return RedirectToAction(nameof(Details));
        }

        var result = await _mediator.Send(new RemoveUserCommand(currentUserId));
        TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Konto zostało usunięte." : result.ErrorMessage;
        return RedirectToAction("Index", "Home");
    }
    #endregion

    private IActionResult RedirectAfterLogin(UserDto user, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var role = user.Role ?? string.Empty;

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

    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        var vm = new ErrorViewModel
        {
            Message = "Brak uprawnień do wyświetlenia tej strony."
        };

        return View("Error", vm);
    }
}