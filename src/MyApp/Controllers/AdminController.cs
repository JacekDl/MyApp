using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

namespace MyApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _users;

        public AdminController(IUserService users)
        {
            _users = users;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _users.GetUsersAsync();
            return View(model);
        }
    }
}
