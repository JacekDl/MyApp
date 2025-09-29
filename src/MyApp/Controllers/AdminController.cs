using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MyApp.Application.Users.Queries;
using MyApp.Application.Reviews.Queries;
using MyApp.Application.Users.Commands;

namespace MyApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : Controller
{

    #region ViewUsers
    public async Task<IActionResult> ViewUsers()
    {
        var dto = await mediator.Send(new GetAllUsersQuery());
        return View(dto);
    }

    #endregion

    #region RemoveUser

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var result = await mediator.Send(new RemoveUserCommand(id));

        if (!result.IsSuccess)
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

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, string? userEmail)
    {
        var dto = await mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;
        ViewBag.UserEmail = userEmail;

        return View(dto);
    }
    #endregion
}       