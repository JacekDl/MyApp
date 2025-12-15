using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.Web.ViewModels;

namespace MyApp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Users
    public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(page, pageSize));
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
                var role = user.Role switch
                {
                    UserRoles.Admin => "Admin",
                    UserRoles.Pharmacist => "Farmaceuta",
                    UserRoles.Patient => "Pacjent",
                    _ => "Nieznana rola"
                };

                var updatedUser = user with { Role = role };
                vm.Users.Add(updatedUser);
            }

            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
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

    public async Task<IActionResult> Reviews(
        string? searchTxt, 
        string? userId, 
        bool? completed, 
        string? userEmail,
        int page = 1,
        int pageSize = 10)
    {
        var result = await _mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail, page, pageSize));

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
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await _mediator.Send(new DeleteReviewCommand(id));
        TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto zalecenia." : result.ErrorMessage;
        return RedirectToAction(nameof(Reviews));
    }
    #endregion
}