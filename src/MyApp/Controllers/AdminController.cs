using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;
using MyApp.Model.enums;
using MyApp.Web.ViewModels;
using System.Security.Claims;

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

        vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Użytkownicy||"]);
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

    #region GetTreatmentPlans
    public async Task<IActionResult> Plans(string? searchTxt, string? userId, TreatmentPlanStatus? status, int page = 1, int pageSize = 10)
    {
        var result = await _mediator.Send(new GetTreatmentPlansQuery(
            searchTxt, 
            userId, 
            status, 
            null,
            null,
            page, 
            pageSize));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        ViewBag.Query = searchTxt;
        ViewBag.Status = status?.ToString().ToLowerInvariant();

        var vm = new TreatmentPlansViewModel();
        if (result.Value is not null)
        {
            vm.Plans = result.Value;
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }
        vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Plany leczenia||"]);
        return View(vm);
    }

    //[HttpPost, ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteReview(int id)
    //{
    //    var result = await _mediator.Send(new DeleteReviewCommand(id));
    //    TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto zalecenia." : result.ErrorMessage;
    //    return RedirectToAction(nameof(Plans));
    //}
    #endregion

    #region GetPlan
    public async Task<IActionResult> GetPlan(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return RedirectToAction("Plans");
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetTreatmentPlanQuery(number, currentUserId));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Plans));
        }

        var tp = result.Value;

        var vm = new TreatmentPlanViewModel();
        vm.Id = tp.Id;
        vm.Number = tp.Number;
        vm.DateCreated = tp.DateCreated;
        vm.DateStarted = tp.DateStarted;
        vm.DateCompleted = tp.DateCompleted;
        vm.IdPharmacist = tp.IdPharmacist;
        vm.IdPatient = tp.IdPatient;
        vm.AdviceFullText = tp.AdviceFullText;
        vm.Status = tp.Status;

        return View(vm);
    }

    #endregion

    #region Promotions
    public async Task<IActionResult> Promotions()
    {
        var result = await _mediator.Send(new GetPendingPromotionsQuery());
        var vm = new PromotionsViewModel();

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(vm);
        }

        if (result.Value is not null)
        {
            vm.Requests = result.Value;
        }

        vm.Breadcrumbs.AddRange(["Start|Users|Admin", "Promocje do roli farmaceuta||"]);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePromotion(int requestId)
    {
        var result = await _mediator.Send(new ApprovePharmacistPromotionCommand(requestId));

        TempData[result.Succeeded ? "Info" : "Error"] =
            result.Succeeded ? "Zgłoszenie zatwierdzone. Użytkownik otrzymał rolę Farmaceuta." : result.ErrorMessage;

        return RedirectToAction(nameof(Promotions));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectPromotion(int requestId)
    {
        var result = await _mediator.Send(new RejectPharmacistPromotionCommand(requestId));

        TempData[result.Succeeded ? "Info" : "Error"] =
            result.Succeeded ? "Zgłoszenie zostało odrzucone." : result.ErrorMessage;

        return RedirectToAction(nameof(Promotions));
    }
    #endregion
}