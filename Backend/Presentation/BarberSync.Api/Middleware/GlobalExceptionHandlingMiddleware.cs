using BarberSync.Api.Models;
using FluentValidation;

namespace BarberSync.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Erro de validação. Path={Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status400BadRequest, "Verifique os dados informados.", ex.Errors.Select(e => (object)new ValidationErrorDto(e.PropertyName, e.ErrorMessage)));
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acesso não autorizado. Path={Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status401Unauthorized, "Acesso não autorizado.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Registro não encontrado. Path={Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status404NotFound, "Registro não encontrado.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Conflito de regra de negócio. Path={Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status409Conflict, "Não foi possível concluir a operação.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Operação cancelada. Path={Path}", context.Request.Path);
            await WriteAsync(context, 499, "Operação cancelada pelo cliente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado. Path={Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente ou contate o suporte.");
        }
    }

    private static Task WriteAsync(HttpContext context, int statusCode, string message, System.Collections.IEnumerable? errors = null)
    {
        context.Response.StatusCode = statusCode;
        var payload = ApiResponse<object>.Fail(message, errors, context.TraceIdentifier);
        return context.Response.WriteAsJsonAsync(payload);
    }
}
