using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

/// <summary>Same-origin gateway for the kiosk. It never invents operational data.</summary>
[Route("KioskApi")]
public sealed partial class KioskApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KioskApiController> logger) : Controller
{
    private const string FlowStateKey = "KioskFlow.State";

    [HttpGet("branches")]
    public Task<IActionResult> Branches([FromQuery] string? deviceCode) => ProxyGetWithDevice("/api/kiosk/branches", deviceCode);

    [HttpGet("services")]
    public Task<IActionResult> Services([FromQuery] string? deviceCode) => ProxyGetWithDevice("/api/kiosk/services", deviceCode);

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals([FromQuery] string? serviceId, [FromQuery] string? deviceCode) =>
        ProxyGetWithDevice($"/api/kiosk/professionals?serviceId={Encode(serviceId)}", deviceCode, true);

    [HttpGet("availability")]
    public Task<IActionResult> Availability([FromQuery] string? branchId, [FromQuery] string? serviceId, [FromQuery] string? professionalId, [FromQuery] DateOnly date, [FromQuery] string? deviceCode) =>
        ProxyGetWithDevice($"/api/kiosk/availability?branchId={Encode(branchId)}&serviceId={Encode(serviceId)}&professionalId={Encode(professionalId)}&date={date:yyyy-MM-dd}", deviceCode, true);

    [HttpPost("session")]
    public Task<IActionResult> Session([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/session", payload);

    [HttpPost("client/find-by-phone")]
    public Task<IActionResult> FindByPhone([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/find-by-phone", payload);

    [HttpPost("client/quick-register")]
    public Task<IActionResult> QuickRegister([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/quick-register", payload);

    [HttpPost("check-in")]
    public Task<IActionResult> CheckIn([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/check-in", payload);

    [HttpPost("pre-orders")]
    public Task<IActionResult> PreOrder([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/pre-orders", payload);

    [HttpGet("flow")]
    public IActionResult Flow() => Content(HttpContext.Session.GetString(FlowStateKey) ?? "{}", "application/json");

    [HttpPut("flow")]
    public IActionResult SaveFlow([FromBody] JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object) return BadRequest(Error("Etapa inválida."));
        HttpContext.Session.SetString(FlowStateKey, payload.GetRawText());
        return Content(payload.GetRawText(), "application/json");
    }

    [HttpDelete("flow")]
    public IActionResult DeleteFlow() { HttpContext.Session.Remove(FlowStateKey); return NoContent(); }

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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("API operacional indisponível. Confirme se BarberSync.Api está rodando e se a URL configurada está correta."));
        }
    }

    private Task<IActionResult> ProxyGetWithDevice(string path, string? queryDeviceCode, bool hasQuery = false)
    {
        var device = ResolveDevice(queryDeviceCode);
        if (!device.IsValid)
            return Task.FromResult<IActionResult>(DeviceProblem(device.ErrorCode!));

        logger.LogInformation("Totem usando DeviceCode originado de {DeviceCodeSource}.", device.Source);
        return ProxyGet($"{path}{(hasQuery ? "&" : "?")}deviceCode={Encode(device.Value)}");
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("API operacional indisponível. Confirme se BarberSync.Api está rodando e se a URL configurada está correta. Nenhum dado ou pagamento foi registrado."));
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

    private DeviceResolution ResolveDevice(string? queryValue)
    {
        var configured = configuration["Kiosk:DeviceCode"];
        var value = string.IsNullOrWhiteSpace(queryValue) ? configured : queryValue;
        var source = string.IsNullOrWhiteSpace(queryValue) ? "Configuration" : "QueryString";
        if (string.IsNullOrWhiteSpace(value)) return new(null, source, "KIOSK_DEVICE_NOT_CONFIGURED");

        value = value.Trim();
        return DeviceCodePattern().IsMatch(value)
            ? new(value, source, null)
            : new(null, source, "KIOSK_DEVICE_INVALID");
    }

    private ObjectResult DeviceProblem(string errorCode)
    {
        var missing = errorCode == "KIOSK_DEVICE_NOT_CONFIGURED";
        var status = missing && HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status503ServiceUnavailable;
        var message = missing ? "Totem não configurado." : "O código informado para o totem é inválido.";
        return StatusCode(status, new
        {
            success = false,
            message,
            errorCode,
            traceId = HttpContext.TraceIdentifier,
            setup = new
            {
                requiredKey = "Kiosk:DeviceCode",
                example = "dotnet user-secrets set \"Kiosk:DeviceCode\" \"KIOSK-LOCAL-001\" --project .\\Web\\BarberSync.KioskWeb\\BarberSync.KioskWeb.csproj"
            }
        });
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]{4,63}$", RegexOptions.CultureInvariant)]
    private static partial Regex DeviceCodePattern();

    private sealed record DeviceResolution(string? Value, string Source, string? ErrorCode)
    {
        public bool IsValid => ErrorCode is null;
    }

    private static string Encode(string? value) => Uri.EscapeDataString(value ?? string.Empty);
    private object Error(string message) => new { success = false, message, traceId = HttpContext.TraceIdentifier, data = (object?)null, errors = Array.Empty<object>() };
}
