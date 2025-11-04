using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;

namespace MyApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : Controller
{

    #region Users
    public async Task<IActionResult> Users()
    {
        var dto = await mediator.Send(new GetAllUsersQuery());
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var result = await mediator.Send(new RemoveUserCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto użytkownika." : result.Error;
        return RedirectToAction(nameof(Users));
    }
    #endregion

    #region Reviews

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, string? userEmail)
    {
        var dto = await mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;
        ViewBag.UserEmail = userEmail;

        return View(dto);
    }

    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await mediator.Send(new DeleteReviewCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto zalecenia." : result.Error;
        return RedirectToAction(nameof(Reviews));
    }
    #endregion
}