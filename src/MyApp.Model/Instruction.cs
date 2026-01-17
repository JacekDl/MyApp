namespace MyApp.Model
{
    public class Instruction
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Text { get; set; } = default!;

        #region Constants
        public const int CodeMaxLength = 32;
        public const int TextMaxLength = 256;
        #endregion
    }
}