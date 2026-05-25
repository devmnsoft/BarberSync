namespace BarberSync.Api.Models;

public record ValidationError(string Field, string Message);

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<ValidationError> Errors { get; init; } = [];
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data, string message, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(string message, IEnumerable<ValidationError>? errors = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList() ?? [],
        TraceId = traceId
    };
}

public record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int Total);
public record ServiceResult<T>(bool Success, string Message, T? Data = default, IReadOnlyCollection<ValidationError>? Errors = null);
