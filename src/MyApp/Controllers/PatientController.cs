using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Web.ViewModels;
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

        if (!result.IsSuccess)
        {
            return NotFound(); //View with error message can be implemented here.
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

        if (!result.IsSuccess)
        {
            return NotFound(); //View with error message can be implemented here.
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
        if (number != vm.Number)
        {
            return BadRequest();
        }

        try
        {
            var result = await _mediator.Send(new UpdateReviewCommand(number, vm.ReviewText));

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(nameof(vm.ReviewText), result.Error!);
                var data = await _mediator.Send(new GetReviewQuery(number));
                if (data.IsSuccess && data.Value is not null)
                {
                    vm.Advice = data.Value.Text;
                    vm.Number = data.Value.Number;
                }
                return View(vm);
            }   
            return RedirectToAction("CompleteEdit", new { number });
        }
        catch(FluentValidation.ValidationException ex)
        {
            foreach (var err in ex.Errors)
            {
                if (string.Equals(err.PropertyName, nameof(UpdateReviewCommand.ReviewText), StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(vm.ReviewText), err.ErrorMessage);
                else if (string.Equals(err.PropertyName, nameof(UpdateReviewCommand.Number), StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(vm.Number), err.ErrorMessage);
                else
                    ModelState.AddModelError(string.Empty, err.ErrorMessage);
            }

            var data = await _mediator.Send(new GetReviewQuery(number));
            if (data.IsSuccess && data.Value is not null)
            {
                vm.Advice = data.Value.Text;
                vm.Number = data.Value.Number;
            }
            return View(vm);
        }

    }

    [AllowAnonymous, HttpGet("/r/complete")]
    public IActionResult CompleteEdit()
    {
        return View();
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
        if (!precheck.IsSuccess) 
        { 
            ModelState.AddModelError(nameof(vm.Number), precheck.Error!); return View(vm); 
        }

        try
        {
            var claim = await _mediator.Send(new ClaimReviewByPatientCommand(vm.Number, currentUserId));
            if (!claim.IsSuccess)
            {
                ModelState.AddModelError(nameof(vm.Number), claim.Error!);
                return View(vm);
            }
            TempData["Info"] = "Token został przypisany do Twojego konta.";
            return RedirectToAction(nameof(Tokens));
        }
        catch(FluentValidation.ValidationException ex)
        {
            foreach (var err in ex.Errors)
                ModelState.AddModelError(nameof(vm.Number), err.ErrorMessage);

            return View(vm);
        }
    }

    #endregion

    #region ViewReviews
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Tokens(string? searchTxt)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = await _mediator.Send(new GetReviewsQuery(searchTxt, currentUserId, null));

        ViewBag.Query = searchTxt;
        return View(dto);
    }
    #endregion
}