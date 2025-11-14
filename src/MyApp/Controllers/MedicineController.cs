using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Instructions.Queries;
using MyApp.Domain.Medicines;
using MyApp.Domain.Medicines.Commands;
using MyApp.Domain.Medicines.Queries;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Instructions;

namespace MyApp.Web.Controllers
{
    [Authorize(Roles = "Admin, Pharmacist")]
    public class MedicineController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IReviewPdfService _pdfService;

        public MedicineController(IMediator mediator, IReviewPdfService pdfService)
        {
            _mediator = mediator;
            _pdfService = pdfService;
        }

        #region Medicines
        public async Task<IActionResult> Medicines()
        {
            var result = await _mediator.Send(new GetMedicinesQuery());
            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Medicines.cshtml", result.Value);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Medicines.cshtml", result.Value);

            return Forbid(); //TODO: change that
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicine(MedicineDto item, CancellationToken ct)
        {
            var result = await _mediator.Send(new AddMedicineCommand(item.Code, item.Name));
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Dodano lek." : result.ErrorMessage;
            return RedirectToAction(nameof(Medicines));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var result = await _mediator.Send(new DeleteMedicineCommand(id));
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto lek." : result.ErrorMessage;
            return RedirectToAction(nameof(Medicines));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ModifyMedicine(int id)
        {
            var result = await _mediator.Send(new GetMedicineQuery(id));
            if (!result.Succeeded)
            {
                TempData["Error"] = "Lek nie został znaleziony.";
                return RedirectToAction(nameof(Medicines));
            }
            return View("~/Views/Admin/ModifyMedicine.cshtml", result.Value);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyMedicine(MedicineDto item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }
            var result = await _mediator.Send(new UpdateMedicineCommand(item.Id, item.Code, item.Name));
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Zaktualizowano lek." : result.ErrorMessage;
            return RedirectToAction(nameof(Medicines));
        }

        [Authorize(Roles = "Pharmacist")]
        public async Task<IActionResult> PrintMedicines()
        {
            var result = await _mediator.Send(new GetMedicinesQuery());

            var pdfBytes = await _pdfService.GenerateMedicinesPdf(result.Value!);
            Response.Headers.ContentDisposition = "inline; filename=medicine.pdf";
            return File(pdfBytes, "medicine/pdf");
        }

        #endregion

        #region Instructions

        public async Task<IActionResult> Instructions()
        {
            var model = await _mediator.Send(new GetInstructionsQuery());
            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Instructions.cshtml", model);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Instructions.cshtml", model);

            return Forbid(); //TODO :change that
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstruction(AddInstructionCommand command)
        {
            var result = await _mediator.Send(command);
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Dodano dawkowanie." : result.ErrorMessage;
            return RedirectToAction(nameof(Instructions));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstruction(int id)
        {
            var result = await _mediator.Send(new DeleteInstructionCommand(id));
            TempData[result.IsSuccess ? "Info" : "Error"] = result.IsSuccess ? "Usunięto dawkowanie." : result.Error;
            return RedirectToAction(nameof(Instructions));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ModifyInstruction(int id)
        {
            var result = await _mediator.Send(new GetInstructionQuery(id));
            if (!result.IsSuccess)
            {
                TempData["Error"] = "Dawkowanie nie zostało odnalezione.";
                return RedirectToAction(nameof(Instructions));
            }
            return View("~/Views/Admin/ModifyInstruction.cshtml", result.Value);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyInstruction(InstructionDto item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }
            var result = await _mediator.Send(new UpdateInstructionCommand(item.Id, item.Code, item.Text));
            return RedirectToAction(nameof(Instructions));
        }

        [Authorize(Roles = "Pharmacist")]
        public async Task<IActionResult> PrintInstructions()
        {
            var dto = await _mediator.Send(new GetInstructionsQuery());

            var pdfBytes = await _pdfService.GenerateInstructionsPdf(dto);
            Response.Headers.ContentDisposition = "inline; filename=instruction.pdf";
            return File(pdfBytes, "instruction/pdf");
        }

        #endregion
    }
}
