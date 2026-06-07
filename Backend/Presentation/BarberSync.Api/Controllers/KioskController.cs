using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/kiosk")]
public sealed class KioskController(EnterpriseDataService data, ILogger<KioskController> logger) : ControllerBase
{
    [HttpGet("services")] public Task<IActionResult> Services([FromQuery] string deviceCode, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await data.ListAsync("services", cancellationToken)).Where(x => IsActive(x) && Flag(x, "visibleOnKiosk", true)), "Serviços do totem carregados.")), cancellationToken);
    [HttpGet("professionals")] public Task<IActionResult> Professionals([FromQuery] string deviceCode, [FromQuery] string? serviceId, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await data.ListAsync("professionals", cancellationToken)).Where(IsActive), "Profissionais do totem carregados.")), cancellationToken);
    [HttpPost("session")] public Task<IActionResult> Session([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("kiosk_sessions", payload, cancellationToken), "Sessão do totem registrada.")));
    [HttpPost("payment")] public Task<IActionResult> Payment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("payments", payload, cancellationToken), "Pagamento mock registrado.")));
    [HttpPost("payment/mock")] public Task<IActionResult> PaymentMock([FromBody] JsonElement payload, CancellationToken cancellationToken) => Payment(payload, cancellationToken);
    [HttpPost("client/quick-register")] public Task<IActionResult> QuickClient([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("clients", payload, cancellationToken), "Cliente rápido cadastrado.")));
    [HttpPost("client/find-by-phone")] public Task<IActionResult> FindClientByPhone([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope((await data.ListAsync("clients", cancellationToken)).FirstOrDefault(), "Consulta de cliente realizada.")));
    [HttpPost("review")] public Task<IActionResult> Review([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("reviews", payload, cancellationToken), "Avaliação registrada.")));

    private Task<IActionResult> WithDevice(string deviceCode, Func<Task<IActionResult>> action, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceCode)) return Task.FromResult<IActionResult>(BadRequest(new { success = false, message = "DeviceCode obrigatório.", data = (object?)null, errors = new[] { new { field = "deviceCode", message = "DeviceCode obrigatório." } } }));
        return Safe(action);
    }
    private async Task<IActionResult> Safe(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (EnterpriseValidationException ex) { return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors }); }
        catch (Exception ex) { logger.LogError(ex, "Erro no KioskController."); return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() }); }
    }
    private static object Envelope(object? data, string message) => new { success = true, message, data, errors = Array.Empty<object>() };
    private static bool IsActive(Dictionary<string, object?> item) => !item.TryGetValue("isActive", out var active) || active is true || active?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    private static bool Flag(Dictionary<string, object?> item, string key, bool defaultValue) => !item.TryGetValue(key, out var flag) ? defaultValue : flag is true || flag?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
}
