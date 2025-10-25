using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.ViewModels;
using System.Security.Claims;
using MyApp.Model;
using System.Text.Json;

namespace MyApp.Controllers;

[Authorize(Roles = "Pharmacist")]
public class PharmacistController(IReviewPdfService pdfService, IMediator mediator, IWebHostEnvironment env) : Controller
{

    #region GenerateReview
    [HttpGet]
    public IActionResult Reviews()
    {
        // existing instructions load
        var instrPath = Path.Combine(env.WebRootPath, "data", "instructions.json");
        var instrJson = System.IO.File.Exists(instrPath) ? System.IO.File.ReadAllText(instrPath) : "{}";
        var instrDict = JsonSerializer.Deserialize<Dictionary<string, string>>(instrJson)
                       ?? new Dictionary<string, string>();
        ViewBag.InstructionMap = instrDict;

        // NEW: medicines load
        var medsPath = Path.Combine(env.WebRootPath, "data", "medicines.json");
        var medsJson = System.IO.File.Exists(medsPath) ? System.IO.File.ReadAllText(medsPath) : "{}";
        var medsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(medsJson)
                       ?? new Dictionary<string, string>();
        ViewBag.MedicineMap = medsDict;

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
        var result = await mediator.Send(new CreateReviewCommand(currentUserId, vm.Advice));

        if(!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError("", result.Error ?? "Could not create review.");
            return View(vm);
        }

        var pdfBytes = await pdfService.GenerateReviewPdf(result.Value!);

        Response.Headers.ContentDisposition = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
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

