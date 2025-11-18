using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Instructions;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Instructions.Queries;
using MyApp.Domain.Medicines;
using MyApp.Domain.Medicines.Commands;
using MyApp.Domain.Medicines.Queries;
using MyApp.Web.ViewModels;

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
            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return View();
            }

            var vm = new MedicinesViewModel();
            if (result.Value is not null)
            {
                vm.Medicines = result.Value;
            }

            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Medicines.cshtml", vm);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Medicines.cshtml", vm);

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
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Medicines));
            }

            var vm = new ModifyMedicineViewModel();
            vm.Medicine = result.Value!;
            vm.Breadcrumbs.AddRange(["Leki|Medicines|Medicine", "Modyfikuj lek|"]);
            return View("~/Views/Admin/ModifyMedicine.cshtml", vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyMedicine(ModifyMedicineViewModel item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }
            var medicine = item.Medicine;
            var result = await _mediator.Send(new UpdateMedicineCommand(medicine.Id, medicine.Code, medicine.Name));
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
            var result = await _mediator.Send(new GetInstructionsQuery());
            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return View();
            }

            var vm = new InstructionsViewModel();
            if(result.Value is not null)
            {
                vm.Instructions = result.Value;
            }

            if (User.IsInRole("Admin"))
                return View("~/Views/Admin/Instructions.cshtml", vm);

            if (User.IsInRole("Pharmacist"))
                return View("~/Views/Pharmacist/Instructions.cshtml", vm);

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
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Usunięto dawkowanie." : result.ErrorMessage;
            return RedirectToAction(nameof(Instructions));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ModifyInstruction(int id)
        {
            var result = await _mediator.Send(new GetInstructionQuery(id));
            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Instructions));
            }
            var vm = new ModifyInstructionViewModel();
            vm.Instruction = result.Value!;
            vm.Breadcrumbs.AddRange(["Dawkowanie|Instructions|Medicine", "Modyfikuj dawkowanie|"]);
            return View("~/Views/Admin/ModifyInstruction.cshtml", vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyInstruction(ModifyInstructionViewModel item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }
            var instruction = item.Instruction;
            var result = await _mediator.Send(new UpdateInstructionCommand(instruction.Id, instruction.Code, instruction.Text));
            TempData[result.Succeeded ? "Info" : "Error"] = result.Succeeded ? "Zaktualizowano dawkowanie." : result.ErrorMessage;
            return RedirectToAction(nameof(Instructions));


        }

        [Authorize(Roles = "Pharmacist")]
        public async Task<IActionResult> PrintInstructions()
        {
            var result = await _mediator.Send(new GetInstructionsQuery());

            var pdfBytes = await _pdfService.GenerateInstructionsPdf(result.Value!);
            Response.Headers.ContentDisposition = "inline; filename=instruction.pdf";
            return File(pdfBytes, "instruction/pdf");
        }

        #endregion
    }
}
