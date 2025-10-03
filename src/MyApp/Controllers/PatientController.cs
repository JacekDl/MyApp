using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Reviews.Commands;
using MyApp.Application.Reviews.Queries;
using MyApp.ViewModels;
using System.Security.Claims;

namespace MyApp.Controllers;

public class PatientController(IMediator mediator) : Controller
{
    #region EditOrRegister

    [AllowAnonymous, HttpGet("/r/{number}")]
    public async Task<IActionResult> PublicAccess(string number)
    {
        var result = await mediator.Send(new GetReviewQuery(number));

        if (!result.IsSuccess)
        {
            return NotFound(); //View with error message can be implemented here.
        }

        var review = result.Value!;
        var vm = new PublicReviewAccessViewModel
        {
            Advice = review.Text,
            Number = review.Number,
        };
        return View(vm);
    }

    #endregion

    #region PublicReview

    [AllowAnonymous, HttpGet("/r/{number}/edit")]
    public async Task<IActionResult> PublicEdit(string number)
    {
        var result = await mediator.Send(new GetReviewQuery(number));

        if (!result.IsSuccess)
        {
            return NotFound(); //View with error message can be implemented here.
        }

        var review = result.Value!;
        var vm = new PublicReviewEditViewModel
        {
            Advice = review.Text,
            Number = review.Number,
            ReviewText = review.ReviewText ?? string.Empty
        };
        return View(vm);
    }


    [AllowAnonymous, HttpPost("/r/{number}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicEdit(string number, PublicReviewEditViewModel vm)
    {
        if (number != vm.Number)
        {
            return BadRequest();
        }

        var result = await mediator.Send(new UpdateReviewCommand(number, vm.ReviewText));

        if (!result.IsSuccess)
        {
            return NotFound(); //TODO: View with error message can be implemented here.
        }

        return RedirectToAction("CompleteEdit", new { number });
    }

    [AllowAnonymous, HttpGet("/r/complete")]
    public IActionResult CompleteEdit()
    {
        return View();
    }
    #endregion

    #region ViewReviews
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Tokens(string? searchTxt)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = await mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, null));

        ViewBag.Query = searchTxt;
        return View(dto);
    }
    #endregion
}