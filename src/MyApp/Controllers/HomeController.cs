using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Users;

namespace MyApp.Web.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(UserRoles.Admin))
            {
                return RedirectToAction("Users", UserRoles.Admin);
            }

            if (User.IsInRole(UserRoles.Pharmacist))
            {
                return RedirectToAction("Tokens", UserRoles.Pharmacist);
            }

            if (User.IsInRole(UserRoles.Patient))
            {
                return RedirectToAction("Tokens", UserRoles.Patient);
            }
        }
        return View();
    }

}