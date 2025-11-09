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
public class PharmacistController(IReviewPdfService pdfService, IMediator mediator) : Controller
{

    #region GenerateReview
    [HttpGet]
    public async Task<IActionResult> ReviewsAsync()
    {
        var refData = await mediator.Send(new GetDictionariesQuery());

        ViewBag.InstructionMap = refData.InstructionMap;
        ViewBag.MedicineMap = refData.MedicineMap;
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

            var result = await mediator.Send(new CreateReviewCommand(currentUserId, vm.Advice));

            if (!result.IsSuccess || result.Value is null)
            {
                ModelState.AddModelError("", result.Error ?? "Nie udało się utworzyć zaleceń.");
                return View(vm);
            }

            var pdfBytes = await pdfService.GenerateReviewPdf(result.Value!);

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
        var dto = await mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, completed));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        return View(dto);
    }
    #endregion  
}

