using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Helpers;
using QuestPDF.Fluent;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
public class UserRoleController : Controller
{
    [HttpGet]
    public IActionResult Reviews()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Reviews(string userText)
    {
        byte[] pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Header().Text("User Review").Bold().FontSize(20);
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text(userText).FontSize(14);
                });
            });
        }).GeneratePdf();

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }

    public IActionResult Tokens()
    {
        return View();
    }
}
