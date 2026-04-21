namespace NLW.Application.Common.Models;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string errorCode, string error) { IsSuccess = false; ErrorCode = errorCode; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorCode, string error) => new(errorCode, error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(ErrorCode!, Error!);
}
