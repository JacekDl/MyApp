namespace MyApp.Domain.Dictionaries
{
    public class DictionaryDto
    {
        public Dictionary<string, string> InstructionMap { get; init; } = new();
        public Dictionary<string, string> MedicineMap { get; init; } = new();

    }
}
