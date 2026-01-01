using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.Users;
using MyApp.Web.ViewModels;
using System.Security.Claims;

namespace MyApp.Web.Controllers;

[Authorize]
public class ConversationController : Controller
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region ConversationDetails
    //[HttpGet]
    //public async Task<IActionResult> Display(string number)
    //{
    //    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    //    var result = await _mediator.Send(new GetConversationQuery(number, currentUserId));

    //    if (!result.Succeeded)
    //    {
    //        return NotFound(); //TODO: zmienić zamiast zwracać 404
    //    }

    //    var markResult = await _mediator.Send(new MarkConversationSeenCommand(number, currentUserId));
    //    if (!markResult.Succeeded)
    //    {
    //        return BadRequest(); //TODO: zmienić zamiast zwracać 400
    //    }
    //    var vm = new DisplayConversationViewModel();
    //    if (result.Value is not null)
    //    {
    //        vm.Conversation = result.Value;
    //    }
    //    var role = User.FindFirstValue(ClaimTypes.Role);
    //    if (role == UserRoles.Admin)
    //    {
    //        vm.Breadcrumbs.AddRange(["Zalecenia|Reviews|Admin", "Rozmowa||"]);
    //    }
    //    else if (role == UserRoles.Pharmacist)
    //    {
    //        vm.Breadcrumbs.AddRange(["Moje zalecenia|Tokens|Pharmacist", "Rozmowa||"]);
    //    }
    //    else
    //    {
    //        vm.Breadcrumbs.AddRange(["Moje zalecenia|Tokens|Patient", "Rozmowa||"]);
    //    }
    //    return View(vm);
    //}
    #endregion


    #region TreatmentPlan
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTreatmentPlanMessage(string number, string text)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new AddTreatmentPlanReviewEntryCommand(number, currentUserId, text));

        TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Wiadomość wysłana." : result.ErrorMessage;

        if (User.IsInRole(UserRoles.Pharmacist))
        {
            return RedirectToAction("GetPlan", "Pharmacist", new { number });
        }
        else if (User.IsInRole(UserRoles.Patient)) 
        {
            return RedirectToAction("GetPlan", "Patient", new { number });
        }
        //TODO: domyslnie w przyszlosci moze byc admin
        return RedirectToAction("GetPlan", "Patient", new { number });

    }

    #endregion
}
