using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Domain.Users;
using MyApp.Web.ViewModels;
using MyApp.Web.ViewModels.Common;
using System.ComponentModel.DataAnnotations;
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

    [AllowAnonymous, HttpGet("/r/{number}")]
    public async Task<IActionResult> PublicAccess(string number)
    {
        var result = await _mediator.Send(new GetReviewQuery(number));

        if (!result.Succeeded)
        {
            var em = new ErrorViewModel();
            em.Message = result.ErrorMessage;
            return View("Error", em);
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
        var result = await _mediator.Send(new GetReviewQuery(number));

        if (!result.Succeeded)
        {
           var em = new ErrorViewModel();
           em.Message = result.ErrorMessage;
           return View("Error", em);
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
        if(!ModelState.IsValid)
        {
            return View(vm);
        }


        if (number != vm.Number)
        {
            return BadRequest(); //TODO: odesłać do strony z błędem
        }
        
        var result = await _mediator.Send(new UpdateReviewCommand(number, vm.ReviewText));

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
        return View(new ClaimPlanViewModel());
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

        var precheck = await _mediator.Send(new GetTreatmentPlanQuery(vm.Number));
        if (!precheck.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Number), precheck.ErrorMessage!);
            return View(vm);
        }


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

    #region ViewReviews
    //[HttpGet]
    //[Authorize(Roles = UserRoles.Patient)]
    //public async Task<IActionResult> Tokens(string? searchTxt, int page = 1, int pageSize = 10)
    //{
    //    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    //    var result = await _mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, null, null, page, pageSize));
    //    if (!result.Succeeded)
    //    {
    //        TempData["Error"] = result.ErrorMessage;
    //        return View();
    //    }

    //    var vm = new ReviewsViewModel();
    //    if (result.Value is not null)
    //    {
    //        vm.Reviews = result.Value;
    //        vm.TotalCount = result.TotalCount;
    //        vm.Page = result.Page;
    //        vm.PageSize = result.PageSize;
    //    }

    //    ViewBag.Query = searchTxt;
    //    return View(vm);
    //}
    #endregion

    #region ViewTreatmentPlans
    [HttpGet]
    [Authorize(Roles = UserRoles.Patient)]
    public async Task<IActionResult> Plans(string? searchTxt, int page = 1, int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetTreatmentPlansQuery(searchTxt, currentUserId, null, null, page, pageSize));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        var vm = new TreatmentPlansViewModel();
        if (result.Value is not null)
        {
            vm.Plans = result.Value;
            vm.TotalCount = result.TotalCount;
            vm.Page = result.Page;
            vm.PageSize = result.PageSize;
        }

        ViewBag.Query = searchTxt;
        return View(vm);
    }
    #endregion
}