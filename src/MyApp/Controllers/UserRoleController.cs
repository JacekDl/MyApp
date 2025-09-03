using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
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
