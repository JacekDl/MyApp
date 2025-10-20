using MyApp.Domain.Abstractions;
using MyApp.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace MyApp.Services;

public class ReviewPdfService : IReviewPdfService
{
    public Task<byte[]> GenerateReviewPdf(Review review)
    {
        var firstEntry = review.Entries?
            .OrderBy(e => e.CreatedUtc)
            .Select(e => e.Text)
            .FirstOrDefault() ?? string.Empty;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Text(t =>
                    {
                        t.Span("Advice: ").Bold();
                        t.Span(firstEntry ?? string.Empty);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Link: ").Bold();
                        t.Span(review.Number ?? string.Empty);
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }
}