using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
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

    #region GetReview

    [Authorize(Roles = "Patient")]
    [HttpGet]
    public IActionResult GetReview()
    {
        return View(new GetReviewViewModel());
    }

    [Authorize(Roles = "Patient")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GetReview(GetReviewViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var precheck = await _mediator.Send(new GetReviewQuery(vm.Number));
        if (!precheck.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Number), precheck.ErrorMessage!); 
            return View(vm);
        }


        var result = await _mediator.Send(new ClaimReviewByPatientCommand(vm.Number, currentUserId));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(vm.Number), result.ErrorMessage!);
            return View(vm);
        }
        TempData["Info"] = "Token został przypisany do Twojego konta.";
        return RedirectToAction(nameof(Tokens));
    }
    #endregion

    #region ViewReviews
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Tokens(string? searchTxt)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, null));
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View();
        }

        var vm = new ReviewsViewModel();
        if (result.Value is not null)
        {
            vm.Reviews = result.Value;
        }

        ViewBag.Query = searchTxt;
        return View(vm);
    }
    #endregion
}