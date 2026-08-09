using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

/// <summary>Same-origin gateway for the kiosk. It never invents operational data.</summary>
[Route("KioskApi")]
public sealed class KioskApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KioskApiController> logger) : Controller
{
    [HttpGet("services")]
    public Task<IActionResult> Services([FromQuery] string? deviceCode) =>
        ProxyGet($"/api/kiosk/services?deviceCode={Encode(RequiredDevice(deviceCode))}");

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals([FromQuery] string? serviceId, [FromQuery] string? deviceCode) =>
        ProxyGet($"/api/kiosk/professionals?serviceId={Encode(serviceId)}&deviceCode={Encode(RequiredDevice(deviceCode))}");

    [HttpPost("session")]
    public Task<IActionResult> Session([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/session", payload);

    [HttpPost("client/find-by-phone")]
    public Task<IActionResult> FindByPhone([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/find-by-phone", payload);

    [HttpPost("client/quick-register")]
    public Task<IActionResult> QuickRegister([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/quick-register", payload);

    [HttpPost("payment")]
    public Task<IActionResult> Payment([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/payment", payload);

    [HttpPost("review")]
    public Task<IActionResult> Review([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/review", payload);

    private async Task<IActionResult> ProxyGet(string path)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").GetAsync(BuildApiUrl(path));
            return await Forward(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API indisponível durante GET {Path}.", path);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("O totem está temporariamente sem conexão. Chame um atendente."));
        }
    }

    private async Task<IActionResult> ProxyPost(string path, JsonElement payload)
    {
        try
        {
            var body = new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json");
            var response = await httpClientFactory.CreateClient("BarberSyncApi").PostAsync(BuildApiUrl(path), body);
            return await Forward(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API indisponível durante POST {Path}.", path);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("Não foi possível concluir a operação. Nenhum dado ou pagamento foi registrado."));
        }
    }

    private static async Task<ContentResult> Forward(HttpResponseMessage response) => new()
    {
        Content = await response.Content.ReadAsStringAsync(),
        ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/json",
        StatusCode = (int)response.StatusCode
    };

    private string BuildApiUrl(string path)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:5080";
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private string RequiredDevice(string? value) => string.IsNullOrWhiteSpace(value)
        ? configuration["Kiosk:DeviceCode"] ?? "KIOSK-001"
        : value.Trim();

    private static string Encode(string? value) => Uri.EscapeDataString(value ?? string.Empty);
    private static object Error(string message) => new { success = false, message, data = (object?)null, errors = Array.Empty<object>() };
}
