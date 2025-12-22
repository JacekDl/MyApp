using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Reviews.Queries;
using MyApp.Web.ViewModels;
using System.Security.Claims;
using MyApp.Domain.Dictionaries.Queries;
using MyApp.Domain.Users;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.TreatmentPlans.Queries;

namespace MyApp.Web.Controllers;

[Authorize(Roles = UserRoles.Pharmacist)]
public class PharmacistController : Controller
{
    private readonly IReviewPdfService _pdfService;
    private readonly IMediator _mediator;

    public PharmacistController(IReviewPdfService pdfService, IMediator mediator)
    {
        _pdfService = pdfService;
        _mediator = mediator;
    }

    #region GenerateTreatmentPlan

    [HttpGet]
    public async Task<IActionResult> TreatmentPlan()
    {
        var result = await _mediator.Send(new GetDictionariesQuery());

        ViewBag.InstructionMap = result.Value!.InstructionMap;
        ViewBag.MedicineMap = result.Value!.MedicineMap;

        return View(new TreatmentPlanCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TreatmentPlan(TreatmentPlanCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var pharmacistId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var medicines = (vm.Medicines ?? [])
            .Where(m => !string.IsNullOrWhiteSpace(m.MedicineName) && 
                        !string.IsNullOrWhiteSpace(m.MedicineDosage) &&
                        !string.IsNullOrWhiteSpace(m.MedicineTimeOfDay))
            .Select(m => new CreateTreatmentPlanMedicineDTO(
                MedicineName: m.MedicineName.Trim(),
                MedicineDosage: m.MedicineDosage.Trim(),
                MedicineFrequency: m.MedicineTimeOfDay.Trim()))
            .ToList();

        var adviceText = string.Join(
            Environment.NewLine,
            (vm.Advices ?? [])
                .Select(a => a.AdviceText?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a)));

        var result = await _mediator.Send(new CreateTreatmentPlanCommand(
            PharmacistId: pharmacistId,
            Medicines: medicines,
            Advice: adviceText));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(vm);
        }

        var pdfBytes = await _pdfService.GenerateTreatmentPlanPdf(result.Value!);

        Response.Headers.ContentDisposition = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");

    }
    #endregion

    #region GetTreatmentPlans
    [HttpGet]
    public async Task<IActionResult> Plans(string? searchTxt, bool? completed, int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetTreatmentPlansQuery(searchTxt, currentUserId, completed, null, page, pageSize));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();

        var vm = new TreatmentPlansViewModel();
        if (result.Value is not null)
        {
            vm.Plans = result.Value;
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }
        return View(vm);
    }
    #endregion
}

