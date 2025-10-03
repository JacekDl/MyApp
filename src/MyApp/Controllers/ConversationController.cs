using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return View(result.Value);
    }
    #endregion
}
