using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

public class UserRoleController : Controller
{
    public IActionResult Reviews()
    {
        return View();
    }

    public IActionResult Tokens()
    {
        return View();
    }
}
