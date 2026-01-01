using MyApp.Model.enums;

namespace MyApp.Web.ViewModels;

public class ReviewEntryViewModel
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public ConversationParty Author { get; set; }
    public string Text { get; set; } = default!;
}
