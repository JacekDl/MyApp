using MyApp.Domain.Reviews;

namespace MyApp.Web.ViewModels;

public class ReviewsViewModel : PagedViewModel
{
    public List<ReviewDto> Reviews { get; set; } = [];
}