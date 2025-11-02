using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Instructions.Queries;
using MyApp.Domain.Medicines;
using MyApp.Domain.Medicines.Commands;
using MyApp.Domain.Medicines.Queries;

namespace MyApp.Web.Controllers
{
    [Authorize(Roles = "Admin, Pharmacist")]
    public class MedicineController(IMediator mediator) : Controller
    {
        #region Medicines

        public async Task<IActionResult> Medicines()
        {
            var dto = await mediator.Send(new GetMedicinesQuery());
            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Medicines.cshtml", dto);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Medicines.cshtml", dto);

            return Forbid();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicine(MedicineDto item)
        {
            var result = await mediator.Send(new AddMedicineCommand(item.Code, item.Name));
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Dodano lek." : result.Error;
            return RedirectToAction(nameof(Medicines));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var result = await mediator.Send(new DeleteMedicineCommand(id));
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto lek." : result.Error;
            return RedirectToAction(nameof(Medicines));
        }

        #endregion

        #region Instructions

        public async Task<IActionResult> Instructions()
        {
            var model = await mediator.Send(new GetInstructionsQuery());
            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Instructions.cshtml", model);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Instructions.cshtml", model);

            return Forbid();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstruction(AddInstructionCommand command)
        {
            var result = await mediator.Send(command);
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Dodano dawkowanie." : result.Error;
            return RedirectToAction(nameof(Instructions));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstruction(int id)
        {
            var result = await mediator.Send(new DeleteInstructionCommand(id));
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto dawkowanie." : result.Error;
            return RedirectToAction(nameof(Instructions));
        }

        #endregion
    }
}
