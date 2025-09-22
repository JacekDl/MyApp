using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IReviewPdfService
{
    Task<byte[]> GenerateReviewPdf(Review review);
}
