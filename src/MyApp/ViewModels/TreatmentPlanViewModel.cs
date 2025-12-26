using MyApp.Domain.TreatmentPlans;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.ViewModels
{
    public class TreatmentPlanViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string? IdPharmacist { get; set; }
        public string? IdPatient { get; set; }
        public string AdviceFullText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Długość leczenia (dni)")]
        [Range(1, 365, ErrorMessage = "Długość leczenia musi być w zakresie 1–365 dni.")]
        public int? TreatmentLengthDays { get; set; }
    }

}