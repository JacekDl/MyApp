using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
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
    [HttpGet]
    public async Task<IActionResult> Display(string number)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetConversationQuery(number, currentUserId));

        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound();
        }

        await _mediator.Send(new MarkConversationSeenCommand(number, currentUserId));
        return View(result.Value);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Display(string number, string text)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new AddConversationEntryCommand(number, currentUserId, text));
        TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Wiadomość wysłana." : result.ErrorMessage;
        return RedirectToAction(nameof(Display), new { number });
    }
    #endregion
}
