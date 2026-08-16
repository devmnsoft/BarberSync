using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[ApiController]
[Route("AdminApi/health")]
public sealed class AdminHealthApiController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AdminHealthApiController> logger) : ControllerBase
{
    [HttpGet("real-data")]
    public Task<IActionResult> RealData(CancellationToken cancellationToken) =>
        ProxyGet("/api/health/real-data", cancellationToken);

    private async Task<IActionResult> ProxyGet(string path, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BarberSyncApi");
            using var response = await client.GetAsync(BuildApiUrl(path), cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
            }

            logger.LogWarning("Admin health proxy GET {Path} falhou com status {StatusCode}", path, response.StatusCode);
            return DependencyProblem((int)response.StatusCode,
                "O diagnóstico da API não pôde ser concluído.",
                "A API respondeu com erro. Consulte o traceId e valide a conexão com o banco de dados.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Admin health proxy GET {Path} não respondeu.", path);
            return DependencyProblem(StatusCodes.Status503ServiceUnavailable,
                "A API está indisponível.",
                "Inicie a API e confirme a configuração do banco de dados antes de tentar novamente.");
        }
    }

    private ObjectResult DependencyProblem(int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = title, Detail = detail };
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return new ObjectResult(problem) { StatusCode = statusCode };
    }

    private string BuildApiUrl(string path)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
