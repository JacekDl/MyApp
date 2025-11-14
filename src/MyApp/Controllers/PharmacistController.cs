using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Web.ViewModels;
using System.Security.Claims;
using MyApp.Domain.Dictionaries.Queries;

namespace MyApp.Web.Controllers;

[Authorize(Roles = "Pharmacist")]
public class PharmacistController : Controller
{
    private readonly IReviewPdfService _pdfService;
    private readonly IMediator _mediator;

    public PharmacistController(IReviewPdfService pdfService, IMediator mediator)
    {
        _pdfService = pdfService;
        _mediator = mediator;
    }

    #region GenerateReview
    [HttpGet]
    public async Task<IActionResult> ReviewsAsync()
    {
        var result = await _mediator.Send(new GetDictionariesQuery());

        ViewBag.InstructionMap = result.InstructionMap;
        ViewBag.MedicineMap = result.MedicineMap;
        return View(new ReviewCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(ReviewCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {

            var result = await _mediator.Send(new CreateReviewCommand(currentUserId, vm.Advice));

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(vm);
            }

            var pdfBytes = await _pdfService.GenerateReviewPdf(result.Value!);

            Response.Headers.ContentDisposition = "inline; filename=review.pdf";
            return File(pdfBytes, "application/pdf");
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach(var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(vm);
        }
    }
    #endregion

    #region ListUsersReviews
    [HttpGet]
    public async Task<IActionResult> Tokens(string? searchTxt, bool? completed)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, completed));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        return View(result.Value);
    }
    #endregion  
}

