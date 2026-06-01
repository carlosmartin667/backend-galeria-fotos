namespace Fotografia.Application.Helpers;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = [];
    public int? StatusCode { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> Fail(string message, params string[] errors)
    {
        return Fail(message, 400, errors);
    }

    public static ApiResponse<T> Fail(string message, int statusCode, params string[] errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> NotFound(string message)
    {
        return Fail(message, 404);
    }

    public static ApiResponse<T> Forbidden(string message)
    {
        return Fail(message, 403);
    }
}
