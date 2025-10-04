using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Reviews.Commands;
using MyApp.Application.Reviews.Queries;
using System.Security.Claims;

namespace MyApp.Controllers;

[Authorize]
public class ConversationController(IMediator mediator) : Controller
{
    #region ConversationDetails
    [HttpGet]
    public async Task<IActionResult> Display(string number)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetConversationQuery(number, currentUserId));

        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound();
        }

        await mediator.Send(new MarkConversationSeenCommand(number, currentUserId));
        return View(result.Value);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Display(string number, string text)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var add = await mediator.Send(new AddConversationEntryCommand(number, currentUserId, text));
        if (!add.IsSuccess)
        {
            var thread = await mediator.Send(new GetConversationQuery(number, currentUserId));
            if (!thread.IsSuccess || thread.Value is null)
            {
                return NotFound();
            }
            ModelState.AddModelError("text", add.Error!);
            return View(thread.Value);
        }
        TempData["Info"] = "Message sent.";
        return RedirectToAction(nameof(Display), new { number });
    }
    #endregion


}
