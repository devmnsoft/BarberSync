using System.Diagnostics;

namespace BarberSync.Api.Middleware;

/// <summary>Adds correlation and tenant context to every request without logging credentials or request bodies.</summary>
public sealed class RequestObservabilityMiddleware(RequestDelegate next, ILogger<RequestObservabilityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        var traceId = context.TraceIdentifier;
        context.Response.Headers["X-Trace-Id"] = traceId;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = traceId,
            ["TenantId"] = Claim(context, "tenant_id", "tenantId"),
            ["BranchId"] = Claim(context, "branch_id", "branchId"),
            ["UserId"] = Claim(context, "sub", "user_id"),
            ["Module"] = Module(context.Request.Path),
            ["Action"] = $"{context.Request.Method} {context.Request.Path}"
        }))
        {
            await next(context);
            logger.LogInformation(
                "Request concluída. Status={Status} ElapsedMs={ElapsedMs:0.0}",
                context.Response.StatusCode,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }

    private static string? Claim(HttpContext context, params string[] names)
        => names.Select(name => context.User.FindFirst(name)?.Value).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string Module(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return segments.Length > 1 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)
            ? segments[1]
            : segments.FirstOrDefault() ?? "root";
    }
}
