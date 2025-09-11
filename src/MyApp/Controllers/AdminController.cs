using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using System.Threading.Tasks;
using MediatR;
using MyApp.Application.Users.Queries;

namespace MyApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _users;
    private readonly IReviewService _reviews;
    private readonly IMediator _mediator;

    public AdminController(IUserService users, IReviewService reviews, IMediator mediator)
    {
        _users = users;
        _reviews = reviews;
        _mediator = mediator;
    }

    #region ViewUsers
    //public async Task<IActionResult> ViewUsers()
    //{
    //    var model = await _users.GetUsersAsync();
    //    return View(model);
    //}

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
    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, CancellationToken ct) 
    {
        var model = await _reviews.GetReviewsAsync(searchTxt, userId, completed, ct);

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;

        return View(model);
    }
    #endregion
}       