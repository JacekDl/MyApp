using MyApp.Domain.Abstractions;
using MyApp.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QRCoder;
using MyApp.Domain.Medicines;
using MyApp.Domain.Instructions;

namespace MyApp.Web.Services;

public class ReviewPdfService : IReviewPdfService
{
    public Task<byte[]> GenerateInstructionsPdf(IReadOnlyList<InstructionDto> dto)
    {
        var rows = (dto ?? Array.Empty<InstructionDto>())
            .OrderBy(m => m.Code)
            .ToList();

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().AlignCenter()
                        .Text(t => t.Span("Dawkowanie").FontSize(20).Bold());

                    col.Item().AlignCenter().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingVertical(6)
                                  .Text("Kod").Bold();

                            header.Cell().BorderBottom(1).PaddingVertical(6)
                                  .Text("Dawkowanie").Bold();
                        });

                        foreach (var m in rows)
                        {
                            table.Cell().BorderBottom(0.5f).PaddingVertical(4)
                                  .Text(m.Code ?? string.Empty);

                            table.Cell().BorderBottom(0.5f).PaddingVertical(4)
                                  .Text(m.Text ?? string.Empty);
                        }
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateMedicinesPdf(IReadOnlyList<MedicineDto> dto)
    {
        var rows = (dto ?? Array.Empty<MedicineDto>())
            .OrderBy(m => m.Code)
            .ToList();

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().AlignCenter()
                        .Text(t => t.Span("Leki").FontSize(20).Bold());

                    col.Item().AlignCenter().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingVertical(6)
                                  .Text("Kod").Bold();

                            header.Cell().BorderBottom(1).PaddingVertical(6)
                                  .Text("Lek").Bold();
                        });

                        foreach (var m in rows)
                        {
                            table.Cell().BorderBottom(0.5f).PaddingVertical(4)
                                  .Text(m.Code ?? string.Empty);

                            table.Cell().BorderBottom(0.5f).PaddingVertical(4)
                                  .Text(m.Name ?? string.Empty);
                        }
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }

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
                        inner.Item().AlignCenter().Text(t => t.Span("Zostaw swoją opinię lub zadaj pytanie kopiując poniższy link lub skanując kod QR: ").Bold());
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