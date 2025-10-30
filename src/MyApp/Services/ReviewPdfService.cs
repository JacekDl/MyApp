using MyApp.Domain.Abstractions;
using MyApp.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QRCoder;

namespace MyApp.Web.Services;

public class ReviewPdfService : IReviewPdfService
{
    public Task<byte[]> GenerateReviewPdf(Review review)
    {
        var firstEntry = review.Entries?
            .OrderBy(e => e.CreatedUtc)
            .Select(e => e.Text)
            .FirstOrDefault() ?? string.Empty;

        var link = $"https://localhost:7231/r/{review.Number}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);

        byte[] qrPng = qrCode.GetGraphic(
            pixelsPerModule: 10,
            drawQuietZones: true
        );

        var date = review.DateCreated.ToString("dd.MM.yyyy");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().AlignCenter().Text(t => t.Span($"Zalecenia ({date})").FontSize(20).Bold());
                    col.Item().Text(t =>
                    {
                        t.Span(firstEntry ?? string.Empty);
                    });

                    col.Item().PaddingTop(150).Column(inner =>
                    {
                        inner.Spacing(12);
                        inner.Item().Text(t => t.Span("Zostaw swoją opinię lub zadaj pytanie kopiując poniższy link lub skanując kod QR: ").Bold());
                        inner.Item().AlignCenter().Text(t =>
                        {
                            t.Span(link)
                             .FontFamily("Courier New");
                        });
                        inner.Item().AlignCenter().Width(140).Image(qrPng);
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }
}