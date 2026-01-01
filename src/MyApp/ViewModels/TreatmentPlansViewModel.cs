using MyApp.Domain.TreatmentPlans;

namespace MyApp.Web.ViewModels
{
    public class TreatmentPlansViewModel : PagedViewModel
    {
        public List<TreatmentPlanListItemDto> Plans { get; set; } = [];
    }

}
