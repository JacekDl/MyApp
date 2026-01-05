using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Abstractions;
using MyApp.Web.ViewModels;
using System.Security.Claims;
using MyApp.Domain.Dictionaries.Queries;
using MyApp.Domain.Users;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Model.enums;

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

        var vm = new TreatmentPlanCreateViewModel();
        vm.Breadcrumbs.AddRange(["Start|Plans|Pharmacist", "Nowy plan||"]);

        return View(vm);
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
    public async Task<IActionResult> Plans(string? searchTxt, TreatmentPlanStatus? status, int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new GetTreatmentPlansQuery(
            searchTxt, 
            currentUserId, 
            status, 
            ConversationParty.Pharmacist,
            null, 
            page, 
            pageSize));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        ViewBag.Query = searchTxt;
        ViewBag.Completed = status?.ToString().ToLowerInvariant();

        var vm = new TreatmentPlansViewModel();
        if (result.Value is not null)
        {
            vm.Plans = result.Value;
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }
        vm.Breadcrumbs.AddRange(["Start|Plans|Pharmacist", "Utworzone||"]);
        return View(vm);
    }
    #endregion

    #region GetPlan

    public async Task<IActionResult> GetPlan(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return RedirectToAction(nameof(Plans));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetTreatmentPlanQuery(number, currentUserId)); 

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Plans));
        }

        if (!string.Equals(result.Value.IdPharmacist, currentUserId, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var tp = result.Value;

        var vm = new TreatmentPlanViewModel
        {
            Id = tp.Id,
            Number = tp.Number,
            DateCreated = tp.DateCreated,
            DateStarted = tp.DateStarted,
            DateCompleted = tp.DateCompleted,
            IdPharmacist = tp.IdPharmacist,
            IdPatient = tp.IdPatient,
            AdviceFullText = tp.AdviceFullText,
            Status = tp.Status,

            ReviewEntries = tp.ReviewEntries
                .Select(e => new ReviewEntryViewModel
                {
                    Id = e.Id,
                    DateCreated = e.DateCreated,
                    Author = e.Author,
                    Text = e.Text
                })
                .ToList()
        };

        vm.Breadcrumbs.AddRange(["Start|Plans|Pharmacist", "Plany|Plans|Pharmacist", "Szczegóły planu||"]);

        return View(vm);
    }

    #endregion

    #region CheckCompliance

    [HttpGet]
    public async Task<IActionResult> CheckCompliance(string number)
    {
        var result = await _mediator.Send(new GetTreatmentPlanComplianceQuery(number));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("GetPlan");
        }

        var dto = result.Value!;

        var vm = new TreatmentPlanComplianceViewModel
        {
            TreatmentPlanId = dto.TreatmentPlanId,
            Number = dto.Number,
            DateStarted = dto.DateStarted,
            Medicines = dto.Medicines
            .Select(m => new MedicineComplianceViewModel
            {
                TreatmentPlanMedicineId = m.TreatmentPlanMedicineId,
                MedicineName = m.MedicineName,
                Percentage = m.Percentage
            })
            .ToList()
        };

        vm.Breadcrumbs.AddRange(["Start|Plans|Pharmacist", "Plany|Plans|Pharmacist", "Zgodność||"]);

        return View(vm);
    }


    #endregion
}

