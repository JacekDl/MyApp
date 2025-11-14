using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;

namespace MyApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Users
    public async Task<IActionResult> Users()
    {
        var dto = await _mediator.Send(new GetAllUsersQuery());
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var result = await _mediator.Send(new RemoveUserCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto użytkownika." : result.Error;
        return RedirectToAction(nameof(Users));
    }
    #endregion

    #region Reviews

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, string? userEmail)
    {
        var dto = await _mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;
        ViewBag.UserEmail = userEmail;

        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReviewCommand(id));
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto zalecenia." : result.Error;
            return RedirectToAction(nameof(Reviews));
        }
        catch (FluentValidation.ValidationException ex)
        {
            TempData["Error"] = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Reviews));
        }
    }
    #endregion
}