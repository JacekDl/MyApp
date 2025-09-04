using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace MyApp.Services;

public class ReviewPdfService : IReviewPdfService
{
    public Task<byte[]> GenerateReviewPdfAsync(string userText, CancellationToken ct = default)
    {
        var pdfBytes = Document.Create(container =>
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

        return Task.FromResult(pdfBytes);
    }
}
