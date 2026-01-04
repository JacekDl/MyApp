using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Domain.Users;
using MyApp.Model.enums;
using MyApp.Web.ViewModels;
using MyApp.Web.ViewModels.Common;
using System.Security.Claims;

namespace MyApp.Web.Controllers;

public class PatientController : Controller
{
    private readonly IMediator _mediator;

    public PatientController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region EditOrRegister

    //[AllowAnonymous, HttpGet("/r/{number}")]
    //public async Task<IActionResult> PublicAccess(string number)
    //{
    //    var result = await _mediator.Send(new GetReviewQuery(number));

    //    if (!result.Succeeded)
    //    {
    //        var em = new ErrorViewModel();
    //        em.Message = result.ErrorMessage;
    //        return View("Error", em);
    //    }

    //    var review = result.Value!;
    //    var vm = new PublicReviewAccessViewModel
    //    {
    //        Advice = review.Text,
    //        Number = review.Number,
    //    };
    //    return View(vm);
    //}

    #endregion

    #region PublicReview

    [AllowAnonymous, HttpGet("/r/{number}")]
    public async Task<IActionResult> PublicEdit(string number)
    {
        var result = await _mediator.Send(new GetTreatmentPlanQuery(number, null));

        if (!result.Succeeded)
        {
            var em = new ErrorViewModel();
            em.Message = result.ErrorMessage;
            return View("Error", em);
        }

        if (string.Equals(result.Value!.Status, "Zakończony", StringComparison.Ordinal))
        {
            var em = new ErrorViewModel();
            em.Message = "Ten kod planu leczenia został już użyty.";
            return View("Error", em);
        }

        var plan = result.Value!;
        var vm = new PublicReviewEditViewModel
        {
            AdviceFullText = plan.AdviceFullText,
            Number = plan.Number,
            ReviewText = string.Empty
        };
        return View(vm);
    }


    [AllowAnonymous, HttpPost("/r/{number}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicEdit(string number, PublicReviewEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (number != vm.Number)
        {
            var em = new ErrorViewModel { Message = "Nieprawidłowy kod" };
            return View("Error", em);
        }

        var result = await _mediator.Send(new UpdateTreatmentPlanCommand(number, vm.ReviewText));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.ReviewText), result.ErrorMessage!);
            return View(vm);
        }

        return RedirectToAction("CompleteEdit", new { number });
    }

    [AllowAnonymous, HttpGet("/r/complete")]
    public IActionResult CompleteEdit()
    {
        var vm = new InfoViewModel();
        vm.Message = "Opinia została przekazana Twojemu farmaceucie.";
        return View("Info", vm);
    }
    #endregion

    #region ClaimPlan

    [Authorize(Roles = UserRoles.Patient)]
    [HttpGet]
    public IActionResult ClaimPlan()
    {
        var vm = new ClaimPlanViewModel();
        vm.Breadcrumbs.AddRange(["Start|Schedule|Patient", "Pobierz nowy||"]);
        return View(vm);
    }

    [Authorize(Roles = UserRoles.Patient)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimPlan(ClaimPlanViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new ClaimTreatmentPlanCommand(vm.Number, currentUserId));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Number), result.ErrorMessage!);
            return View(vm);
        }
        TempData["Info"] = "Pobrano pomyślnie plan lecznia.";
        return RedirectToAction(nameof(Plans));
    }
    #endregion

    #region ViewPlans
    [HttpGet]
    [Authorize(Roles = UserRoles.Patient)]
    public async Task<IActionResult> Plans(string? searchTxt, TreatmentPlanStatus? status, int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetTreatmentPlansQuery(
            searchTxt, 
            currentUserId, 
            status, 
            ConversationParty.Patient,
            null,
            page, 
            pageSize));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        ViewBag.Query = searchTxt;
        ViewBag.Status = status?.ToString().ToLowerInvariant();

        var vm = new TreatmentPlansViewModel();
        if (result.Value is not null)
        {
            vm.Plans = result.Value;
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }

        vm.Breadcrumbs.AddRange(["Start|Schedule|Patient", "Pobrane plany||"]);

        return View(vm);
    }
    #endregion

    #region GetPlan

    public async Task<IActionResult> GetPlan(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return RedirectToAction("Plans");
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetTreatmentPlanQuery(number, currentUserId));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Plans));
        }

        if (!string.Equals(result.Value.IdPatient, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var tp = result.Value;

        var vm = new TreatmentPlanViewModel();
        vm.Id = tp.Id;
        vm.Number = tp.Number;
        vm.DateCreated = tp.DateCreated;
        vm.DateStarted = tp.DateStarted;
        vm.DateCompleted    = tp.DateCompleted;
        vm.IdPharmacist = tp.IdPharmacist;
        vm.IdPatient = tp.IdPatient;
        vm.AdviceFullText = tp.AdviceFullText;
        vm.Status = tp.Status;
        vm.ReviewEntries = tp.ReviewEntries
            .Select(e => new ReviewEntryViewModel
            {
                Id = e.Id,
                DateCreated = e.DateCreated,
                Author = e.Author,
                Text = e.Text
            })
            .ToList();
        
        return View(vm);
    }

    #endregion

    #region UpdatePlanStart
    [Authorize(Roles = UserRoles.Patient)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePlanStart(TreatmentPlanViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("GetPlan", vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Number))
        {
            return BadRequest();
        }

        if (!vm.DateStarted.HasValue || !vm.DateCompleted.HasValue)
        {
            ModelState.AddModelError("", "Wybierz datę rozpoczęcia i długość leczenia aby wyliczyć datę ukończenia.");
            return View("GetPlan", vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new UpdateTreatmentPlanStartCommand(
            Number: vm.Number,
            IdPatient: currentUserId,
            DateStarted: vm.DateStarted.Value,
            DateCompleted: vm.DateCompleted.Value
        ));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(GetPlan), new { number = vm.Number });
        }

        TempData["Info"] = "Zapisano daty planu leczenia.";
        return RedirectToAction(nameof(Plans));
    }
    #endregion

    #region Calendar
    [Authorize(Roles = UserRoles.Patient)]
    [HttpGet]
    public async Task<IActionResult> Schedule(DateTime? date)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var selectedDate = (date ?? DateTime.Today).Date;

        var result = await _mediator.Send(new GetTreatmentPlanMedicinesQuery(currentUserId, selectedDate));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new DateMedicinesViewModel { Date = selectedDate });
        }

        var vm = DateMedicinesViewModel.From(result.Value ?? new());
        vm.Date = selectedDate;

        var takenIdsResult = await _mediator.Send(new GetTakenMedicineIdsForDateQuery(currentUserId, selectedDate));
        vm.TakenMedicineIds = takenIdsResult.Value ?? new HashSet<int>();
        vm.Breadcrumbs.AddRange(["Start|Schedule|Patient", "Kalendarz||"]);
        return View(vm);
    }

    [Authorize(Roles = UserRoles.Patient)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMedicineTaken(int treatmentPlanMedicineId, DateTime date, bool isTaken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _mediator.Send(new ToggleMedicineTakenCommand(
            IdPatient: currentUserId,
            TreatmentPlanMedicineId: treatmentPlanMedicineId,
            Date: date.Date,
            IsTaken: isTaken
        ));

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Schedule), new { date = date.Date });
    }
    #endregion
}