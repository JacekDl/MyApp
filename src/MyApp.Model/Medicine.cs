namespace MyApp.Model
{
    public class Medicine
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;

        #region Constants
        public const int CodeMaxLength = 32;
        public const int NameMaxLength = 128;
        #endregion
    }
}
