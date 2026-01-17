using MyApp.Model.enums;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Model
{
    public class ReviewEntry
    {
        public int Id { get; set; }
        public int IdTreatmentPlanReview { get; set; }
        public TreatmentPlanReview TreatmentPlanReview { get; set; } = default!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public ConversationParty Author { get; set; } = default!;

        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = default!;

        #region Constants
        public const int TextMaxLength = 500;
        #endregion
    }
}
