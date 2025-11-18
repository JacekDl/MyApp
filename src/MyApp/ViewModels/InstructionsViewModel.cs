using MyApp.Domain.Instructions;

namespace MyApp.Web.ViewModels;

public class InstructionsViewModel : ViewModelBase
{
    public IReadOnlyList<InstructionDto> Instructions { get; set; } = [];
}