using MyApp.Domain.Instructions;

namespace MyApp.Web.ViewModels;

public class ModifyInstructionViewModel : ViewModelBase
{
    public InstructionDto Instruction { get; set; } = null!;
}
