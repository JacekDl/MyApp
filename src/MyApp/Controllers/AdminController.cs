using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

namespace MyApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _users;

    public AdminController(IUserService users)
    {
        _users = users;
    }

    #region ViewUsers
    public async Task<IActionResult> ViewUsers()
    {
        var model = await _users.GetUsersAsync();
        return View(model);
    }

    #endregion

    #region RemoveUser
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var result = await _users.RemoveUserAsync(id);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Info"] = "User removed";
        }

        return RedirectToAction(nameof(ViewUsers));
    }
    #endregion

    #region ViewReviews
    public IActionResult ViewReviews()
    {
        return View();
    }
    #endregion
}