using MyApp.Domain.Reviews;

namespace MyApp.Web.ViewModels;

public class ReviewsViewModel : ViewModelBase
{
    public IReadOnlyList<ReviewDto> Reviews { get; set; } = [];
}