using MyApp.Domain.Instructions;

namespace MyApp.Web.ViewModels;

public class InstructionsViewModel : PagedViewModel
{
    public IReadOnlyList<InstructionDto> Instructions { get; set; } = [];
}