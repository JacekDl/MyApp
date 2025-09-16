using MyApp.Domain;

namespace MyApp.Services;

public interface IReviewPdfService
{
    Task<byte[]> GenerateReviewPdf(Review review);
}
