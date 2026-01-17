namespace MyApp.Model
{
    public class PharmacistPromotionRequest
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string NumerPWZF { get; set; } = default!;

        public string Status { get; set; } = "Pending";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public static implicit operator PharmacistPromotionRequest(List<PharmacistPromotionRequest> v)
        {
            throw new NotImplementedException();
        }

        #region Constants
        public const int FirstNameMaxLength = 50;
        public const int LastNameMaxLength = 80;
        public const int NumerPWZFLength = 8;
        #endregion
    }
}
