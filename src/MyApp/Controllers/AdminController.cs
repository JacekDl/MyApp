using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.Web.ViewModels;

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
        var result = await _mediator.Send(new GetAllUsersQuery());
        var vm = new UsersViewModel();
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(vm);
        }
        else if (result.Value is not null)
        {
            foreach (var user in result.Value)
            {
                vm.Users.Add(user);
            }
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var result = await _mediator.Send(new RemoveUserCommand(id));
        TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto użytkownika." : result.ErrorMessage;
        return RedirectToAction(nameof(Users));
    }
    #endregion

    #region Reviews

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, string? userEmail)
    {
        var result = await _mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new ReviewsViewModel()); //TODO: is that correct?
        }

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;
        ViewBag.UserEmail = userEmail;

        var vm = new ReviewsViewModel();
        if (result.Value is not null)
        {
            vm.Reviews = result.Value;
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteReviewCommand(id));
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto zalecenia." : result.ErrorMessage;
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