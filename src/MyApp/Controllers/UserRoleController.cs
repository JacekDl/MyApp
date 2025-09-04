using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using System.Threading.Tasks;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
public class UserRoleController : Controller
{
    private readonly IReviewPdfService _pdfService;

    public UserRoleController(IReviewPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpGet]
    public IActionResult Reviews()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(string userText, CancellationToken ct)
    {
        var pdfBytes = await _pdfService.GenerateReviewPdfAsync(userText, ct);

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }

    public IActionResult Tokens()
    {
        return View();
    }
}
