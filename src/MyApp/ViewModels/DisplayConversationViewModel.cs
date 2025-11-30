using MyApp.Domain.Reviews;

namespace MyApp.Web.ViewModels
{
    public class DisplayConversationViewModel : ViewModelBase
    {
        public ConversationDto? Conversation { get; set; } = null!;
    }
}
