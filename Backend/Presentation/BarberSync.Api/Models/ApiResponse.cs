namespace BarberSync.Api.Models;

public record ValidationErrorDto(string Field, string Message);

public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
    public List<string> Errors { get; init; } = [];
    public string? TraceId { get; init; }

    public static ApiResponse Ok(string message, object? data = null, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList() ?? [],
        TraceId = traceId
    };
}

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data, string message, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList() ?? [],
        TraceId = traceId
    };
}

public record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int Total);

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];

    public static ServiceResult<T> Ok(T? data, string message) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ServiceResult<T> Fail(string message, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList() ?? []
    };
}

public class ErrorResponse
{
    public string Code { get; init; } = "unexpected_error";
    public string Message { get; init; } = "Ocorreu um erro inesperado. Tente novamente ou contate o suporte.";
    public string? TraceId { get; init; }
}
