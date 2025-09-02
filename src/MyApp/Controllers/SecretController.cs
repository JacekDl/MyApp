using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

[Authorize]
public class SecretController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
