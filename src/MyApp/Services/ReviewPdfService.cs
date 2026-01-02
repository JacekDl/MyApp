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
    public Task<byte[]> GenerateTreatmentPlanPdf(TreatmentPlan plan)
    {
        
        var host = "https://localhost:7231";
        var link = $"{host}/r/{plan.Number}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);

        byte[] qrPng = qrCode.GetGraphic(
            pixelsPerModule: 10,
            drawQuietZones: true
        );

        var date = plan.DateCreated.ToString("dd.MM.yyyy");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().AlignCenter().Text(t => t.Span($"Twój plan leczenia").FontSize(20).Bold());
                    col.Item().AlignCenter().Text(t => t.Span($"{date}"));

                    col.Item()
                               .Border(1)
                               .BorderColor(Colors.Grey.Darken2)
                               .Padding(16)
                               .ExtendVertical()
                               .Column(inner =>
                               {
                                   inner.Spacing(12);

                                   inner.Item().Text(t =>
                                   {
                                       t.Span(plan.AdviceFullText ?? string.Empty);
                                   });

                                   inner.Item()
                                     .PaddingVertical(8)
                                     .LineHorizontal(1)
                                     .LineColor(Colors.Grey.Darken2);

                                   inner.Item()
                                        .ExtendVertical()
                                        .AlignBottom()
                                        .Column(footer =>
                                        {
                                            footer.Spacing(12);

                                            footer.Item().AlignCenter().Text(t =>
                                                t.Span("Zostaw swoją opinię kopiując poniższy link lub skanując kod QR:")
                                                 .Bold());

                                            footer.Item().AlignCenter().Text(t =>
                                                t.Span(link).FontFamily("Courier New"));

                                            footer.Item().AlignCenter().Width(140).Image(qrPng);

                                            footer.Item().AlignCenter().Text(t =>
                                                t.Span($"Możesz też pobrać swój plan leczenia po zarejestrowaniu na stronie {host} i wpisaniu kodu:")
                                                 .Bold());

                                            footer.Item().AlignCenter().Text(t =>
                                                t.Span(plan.Number)
                                                 .FontFamily("Courier New")
                                                 .FontSize(14));
                                        });
                               });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }

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
}