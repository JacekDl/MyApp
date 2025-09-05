using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;
using System.Security.Cryptography;

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

    #region Generate Review
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

        var review = await _reviewService.CreateAsync(model.Advice, ct);
        var pdfBytes = await _pdfService.GenerateReviewPdfAsync(review, ct);

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }
    #endregion

    #region Public Edit Review
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
    public IActionResult Tokens()
    {
        return View();
    }
}
