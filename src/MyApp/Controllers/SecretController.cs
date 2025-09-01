using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

public class SecretController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
