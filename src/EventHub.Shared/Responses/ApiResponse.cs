namespace EventHub.Shared.Responses;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Failure(string message, string? code = null) => new()
    {
        Success = false,
        Message = message,
        Code = code
    };
}

public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string message, string? code = null) => new()
    {
        Success = false,
        Message = message,
        Code = code
    };
}
