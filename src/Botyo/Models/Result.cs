namespace Botyo.Models;

public record Result<T>
{
    public int StatusCode { get; set; }
    public T? Payload { get; set; }
    public Exception? Error { get; set; }

    public static Result<T> Success(int statusCode)
        => new()
        {
            StatusCode = statusCode
        };

    public static Result<T> Success(int statusCode, T payload)
        => new()
        {
            StatusCode = statusCode,
            Payload = payload
        };

    public static Result<T> Failure(int statusCode, Exception error)
        => new()
        {
            StatusCode = statusCode,
            Error = error
        };
}