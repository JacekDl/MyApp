using MyApp.Domain.Users;
using MyApp.Model;

namespace MyApp.Web.ViewModels;

public class PromotionsViewModel : ViewModelBase
{
    public IReadOnlyList<PendingPromotionsDto> Requests { get; set; } = [];
}
