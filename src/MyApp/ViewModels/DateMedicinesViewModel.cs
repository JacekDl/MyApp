using MyApp.Domain.TreatmentPlans;
using MyApp.Model.enums;

namespace MyApp.Web.ViewModels
{
    public class DateMedicinesViewModel : ViewModelBase
    {
        public List<TreatmentPlanMedicineDto> Morning { get; init; } = new();
        public List<TreatmentPlanMedicineDto> Noon { get; init; } = new();
        public List<TreatmentPlanMedicineDto> Afternoon { get; init; } = new();
        public List<TreatmentPlanMedicineDto> Evening { get; init; } = new();

        public DateTime Date { get; set; } = DateTime.Today;

        public HashSet<int> TakenMedicineIds { get; set; } = new();

        public static DateMedicinesViewModel From(List<TreatmentPlanMedicineDto> items)
        {
            return new DateMedicinesViewModel
            {
                Morning = items.Where(x => x.TimeOfDay == TimeOfDay.Rano).ToList(),
                Noon = items.Where(x => x.TimeOfDay == TimeOfDay.Poludnie).ToList(),
                Afternoon = items.Where(x => x.TimeOfDay == TimeOfDay.Popoludnie).ToList(),
                Evening = items.Where(x => x.TimeOfDay == TimeOfDay.Wieczor).ToList(),
            };
        }
    }
}
