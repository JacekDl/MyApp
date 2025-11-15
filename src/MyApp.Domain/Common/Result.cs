namespace MyApp.Domain.Common
{
    public record class Result
    {
        public bool Succeeded => string.IsNullOrEmpty(ErrorMessage);
        public string? ErrorMessage { get; init; }
    }

    public record class Result<TValue> : Result
    {
        public TValue? Value { get; init; }
    }
}