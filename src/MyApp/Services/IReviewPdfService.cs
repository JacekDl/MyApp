namespace MyApp.Services;

public interface IReviewPdfService
{
    Task<byte[]> GenerateReviewPdfAsync(string userText, CancellationToken ct = default);
}
