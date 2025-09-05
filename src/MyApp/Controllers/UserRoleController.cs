using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;
using System.Security.Cryptography;

namespace MyApp.Controllers;

[Authorize(Roles = "User")]
public class UserRoleController : Controller
{
    private readonly IReviewPdfService _pdfService;
    private readonly ApplicationDbContext _db;

    public UserRoleController(IReviewPdfService pdfService, ApplicationDbContext db)
    {
        _pdfService = pdfService;
        _db = db;
    }

    [HttpGet]
    public IActionResult Reviews()
    {
        return View(new ReviewCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(ReviewCreateViewModel model, CancellationToken ct)
    {
        string number;
        do
        {
            number = GenerateDigits(10);
        }
        while (await _db.Reviews.AnyAsync(r => r.Number == number, ct));

        var review = new Review
        {
            Advice = model.Advice,
            Number = number,
            Completed = false
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(ct);

        var pdfBytes = await _pdfService.GenerateReviewPdfAsync(review, ct);

        Response.Headers["Content-Disposition"] = "inline; filename=review.pdf";
        return File(pdfBytes, "application/pdf");
    }

    private static string GenerateDigits(int digits = 10)
    {
        var chars = new char[digits];
        for (int i = 0; i < digits; i++)
        {
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }
        return new string(chars);
    }

    public IActionResult Tokens()
    {
        return View();
    }
}
