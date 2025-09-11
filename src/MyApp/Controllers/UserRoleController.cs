using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Services;
using MyApp.ViewModels;
using System.Security.Claims;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
public class UserRoleController : Controller
{
    private readonly IReviewPdfService _pdfService;
    private readonly IReviewService _reviewService;

    public UserRoleController(IReviewPdfService pdfService, IReviewService reviewService)
    {
        _pdfService = pdfService;
        _reviewService = reviewService;
    }

    #region GenerateReview
    [HttpGet]
    public IActionResult Reviews()
    {
        return View(new ReviewCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(ReviewCreateViewModel model, CancellationToken ct)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var review = await _reviewService.CreateAsync(currentUserId, model.Advice, ct);
        var pdfBytes = await _pdfService.GenerateReviewPdfAsync(review, ct);

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }
    #endregion

    #region PublicEditReview
    [AllowAnonymous, HttpGet("/r/{number}")]
    public async Task<IActionResult> PublicEdit(string number, CancellationToken ct)
    { 
        var review = await _reviewService.GetPublicAsync(number, ct);

        if (review == null)
        {
            return NotFound();
        }

        ViewBag.Advice = review.Advice;
        var model = new PublicReviewEditViewModel
        {
            Number = review.Number,
            ReviewText = review.ReviewText ?? string.Empty
        };
        return View(model);
    }

    [AllowAnonymous, HttpPost("/r/{number}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicEdit(string number, PublicReviewEditViewModel model, CancellationToken ct)
    {
        if (number != model.Number)
        {
            return BadRequest();
        }

        var ok = await _reviewService.UpdatePublicAsync(number, model.ReviewText, ct);
        if (!ok)
        {
            return NotFound();
        }

        TempData["Saved"] = true;
        return RedirectToAction(nameof(PublicEdit), new { number });
    }


    #endregion

    #region ListUsersReviews
    [HttpGet]
    public async Task<IActionResult> Tokens(string? searchTxt, bool? completed, CancellationToken ct)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var reviews = await _reviewService.GetByCreatorAsync(currentUserId, searchTxt, completed, ct);
        var model = reviews.Select(r => new TokenItemViewModel
        {
            Id = r.Id,
            Number = r.Number,
            Advice = r.Advice,
            DateCreated = r.DateCreated,
            Completed = r.Completed,
            ReviewText = r.ReviewText ?? string.Empty
        }).ToList();

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();

        return View(model);
    }
}

#endregion