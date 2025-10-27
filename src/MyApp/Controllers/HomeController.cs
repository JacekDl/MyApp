using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Users", "Admin");
            }

            if (User.IsInRole("Pharmacist"))
            {
                return RedirectToAction("Reviews", "Pharmacist");
            }
            else return RedirectToAction("Tokens", "Patient");
        }
        return View();
    }

}