using MyApp.Domain.Instructions;
using MyApp.Domain.Medicines;
using MyApp.Model;

namespace MyApp.Domain.Abstractions;

public interface IReviewPdfService
{
    Task<byte[]> GenerateInstructionsPdf(IReadOnlyList<InstructionDto> dto);
    Task<byte[]> GenerateMedicinesPdf(IReadOnlyList<MedicineDto> dto);
    Task<byte[]> GenerateReviewPdf(Review review);
}
