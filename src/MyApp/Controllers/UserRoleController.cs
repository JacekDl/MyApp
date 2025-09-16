using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Reviews.Commands;
using MyApp.Application.Reviews.Queries;
using MyApp.Models;
using MyApp.Services;
using MyApp.ViewModels;
using System.Security.Claims;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
public class UserRoleController : Controller
{
    private readonly IReviewPdfService _pdfService;
    private readonly IMediator _mediator;

    public UserRoleController(IReviewPdfService pdfService, IMediator mediator)
    {
        _pdfService = pdfService;
        _mediator = mediator;
    }

    #region GenerateReview
    [HttpGet]
    public IActionResult Reviews()
    {
        return View(new ReviewCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(ReviewCreateViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new CreateReviewCommand(currentUserId, vm.Advice));


        var pdfBytes = await _pdfService.GenerateReviewPdfAsync(result.Value!, ct);

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }
    #endregion

    #region PublicEditReview

    [AllowAnonymous, HttpGet("/r/{number}")]
    public async Task<IActionResult> PublicEdit(string number)
    {
        var result = await _mediator.Send(new GetReviewQuery(number));

        if (!result.IsSuccess)
        {
            return NotFound(); //View with error message can be implemented here.
        }

        var review = result.Value!;
        var vm = new PublicReviewEditViewModel
        {
            Advice = review.Advice,
            Number = review.Number,
            ReviewText = review.ReviewText ?? string.Empty
        };
        return View(vm);
    }


    [AllowAnonymous, HttpPost("/r/{number}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicEdit(string number, PublicReviewEditViewModel vm)
    {
        if (number != vm.Number)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new UpdateReviewCommand(number, vm.ReviewText));

        if (!result.IsSuccess)
        {
            return NotFound(); //TODO: View with error message can be implemented here.
        }

        TempData["Saved"] = true;
        return RedirectToAction("CompleteEdit", new {number});
    }

    [AllowAnonymous, HttpGet("/r/{number}/complete")]
    public IActionResult CompleteEdit(string number)
    {
        return View();
    }


    #endregion

    #region ListUsersReviews
    [HttpGet]
    public async Task<IActionResult> Tokens(string? searchTxt, bool? completed)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dto = await _mediator.Send(new GetReviewsQuery(searchTxt, currentUserId.ToString(), completed));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        return View(dto);
    }
}

#endregion