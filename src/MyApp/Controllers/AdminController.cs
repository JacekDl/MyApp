using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Instructions.Queries;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users.Commands;
using MyApp.Domain.Users.Queries;

namespace MyApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : Controller
{

    #region Users
    public async Task<IActionResult> Users()
    {
        var dto = await mediator.Send(new GetAllUsersQuery());
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var result = await mediator.Send(new RemoveUserCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "User removed." : result.Error;
        return RedirectToAction(nameof(Users));
    }
    #endregion

    #region Reviews

    public async Task<IActionResult> Reviews(string? searchTxt, string? userId, bool? completed, string? userEmail)
    {
        var dto = await mediator.Send(new GetReviewsQuery(searchTxt, userId, completed, userEmail));

        ViewBag.Query = searchTxt;
        ViewBag.Completed = completed?.ToString().ToLowerInvariant();
        ViewBag.UserId = userId;
        ViewBag.UserEmail = userEmail;

        return View(dto);
    }

    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await mediator.Send(new DeleteReviewCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Review deleted." : result.Error;
        return RedirectToAction(nameof(Reviews));
    }
    #endregion

    //#region Medicines

    //public async Task<IActionResult> Medicines()
    //{
    //    var dto = await mediator.Send(new GetMedicinesQuery());
    //    return View(dto);
    //}

    //[HttpPost, ValidateAntiForgeryToken]
    //public async Task<IActionResult> AddMedicine(MedicineDto item)
    //{
    //    var result = await mediator.Send(new AddMedicineCommand(item.Code, item.Name));
    //    TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Medicine added." : result.Error;
    //    return RedirectToAction(nameof(Medicines));
    //}

    //[HttpPost, ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteMedicine(int id)
    //{
    //    var result = await mediator.Send(new DeleteMedicineCommand(id));
    //    TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Medicine deleted." : result.Error;
    //    return RedirectToAction(nameof(Medicines));
    //}

    //#endregion

    #region Instructions

    public async Task<IActionResult> Instructions()
    {
        var model = await mediator.Send(new GetInstructionsQuery());
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInstruction(AddInstructionCommand command)
    {
        var result = await mediator.Send(command);
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Instruction added." : result.Error;
        return RedirectToAction(nameof(Instructions));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteInstruction(int id)
    {
        var result = await mediator.Send(new DeleteInstructionCommand(id));
        TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Instruction deleted." : result.Error;
        return RedirectToAction(nameof(Instructions));
    }

    #endregion
}