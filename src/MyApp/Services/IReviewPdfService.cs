using MyApp.Domain;

namespace MyApp.Services;

public interface IReviewPdfService
{
    Task<byte[]> GenerateReviewPdfAsync(Review review, CancellationToken ct = default);
}
