namespace MyApp.Services;

public class OperationResult
{
    public bool Succeeded { get; protected set; }
    public string? Error { get; protected set; }

    public static OperationResult Success() => new() { Succeeded = true };
    public static OperationResult Failure(string error) => new() { Succeeded = false, Error = error };

}

public class OperationResult<T> : OperationResult
{
    public T? Value { get; protected set; }
    public static OperationResult<T> Success(T value) => new() { Succeeded = true, Value = value };
    public new static OperationResult<T> Failure(string error) => new() { Succeeded = false, Error = error };
}