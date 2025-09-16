using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using MediatR;
using MyApp.Application.Users.Queries;
using MyApp.Application.Reviews.Queries;
using MyApp.Application.Users.Commands;

namespace MyApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IUserService users, IMediator mediator)
    {
        _mediator = mediator;
    }

    #region ViewUsers
    public async Task<IActionResult> ViewUsers()
    {
        var dto = await _mediator.Send(new GetAllUsersQuery());
        return View(dto);
    }

    #endregion

    #region RemoveUser
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var result = await _mediator.Send(new RemoveUserCommand(id));

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

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed)
    {
        var dto = await _mediator.Send(new GetReviewsQuery(searchTxt, userId, completed));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;

        return View(dto);
    }
    #endregion
}       