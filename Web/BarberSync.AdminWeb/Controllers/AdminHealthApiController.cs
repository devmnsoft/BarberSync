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
    public Task<IActionResult> RealData() => ProxyGet(
        "/api/health/real-data",
        new
        {
            success = false,
            databaseConnected = false,
            schemaReady = false,
            realDataReady = false,
            message = "Health de dados reais indisponível via proxy AdminApi.",
            resources = Array.Empty<object>(),
            isDemo = true
        });

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BarberSyncApi");
            var response = await client.GetAsync(BuildApiUrl(path));
            if (response.IsSuccessStatusCode)
            {
                return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
            }

            logger.LogWarning("Admin health proxy GET {Path} falhou com status {StatusCode}", path, response.StatusCode);
            return Ok(fallback);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Admin health proxy GET {Path} lançou exceção. Usando fallback controlado.", path);
            return Ok(fallback);
        }
    }

    private string BuildApiUrl(string path)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
