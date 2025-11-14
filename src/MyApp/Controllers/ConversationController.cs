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

        if (!result.Succeeded)
        {
            return NotFound(); //TODO: zmienić zamiast zwracać 404
        }

        var markResult = await _mediator.Send(new MarkConversationSeenCommand(number, currentUserId));
        if (!markResult.Succeeded)
        {
            return BadRequest(); //TODO: zmienić zamiast zwracać 400
        }
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
