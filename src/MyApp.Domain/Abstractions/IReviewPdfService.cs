using MyApp.Model;

namespace MyApp.Domain.Abstractions;

public interface IReviewPdfService
{
    Task<byte[]> GenerateReviewPdf(Review review);
}
