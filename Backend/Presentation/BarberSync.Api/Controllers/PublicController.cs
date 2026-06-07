using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController(EnterpriseDataService data, ILogger<PublicController> logger) : ControllerBase
{
    [HttpGet("services")] public Task<IActionResult> Services(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope((await data.ListAsync("services", cancellationToken)).Where(x => IsActive(x) && Flag(x, "visibleOnPublicWeb", true)), "Serviços públicos carregados.")));
    [HttpGet("professionals")] public Task<IActionResult> Professionals(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope((await data.ListAsync("professionals", cancellationToken)).Where(IsActive), "Profissionais públicos carregados.")));
    [HttpPost("appointments")] public Task<IActionResult> Appointment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.PublicAppointmentAsync(payload, cancellationToken), "Agendamento público criado com sucesso.")));
    [HttpPost("leads")] public Task<IActionResult> Lead([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("public_leads", payload, cancellationToken), "Lead público registrado com sucesso.")));

    private async Task<IActionResult> Safe(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (EnterpriseValidationException ex) { return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors }); }
        catch (Exception ex) { logger.LogError(ex, "Erro no PublicController."); return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() }); }
    }
    private static object Envelope(object? data, string message) => new { success = true, message, data, errors = Array.Empty<object>() };
    private static bool IsActive(Dictionary<string, object?> item) => !item.TryGetValue("isActive", out var active) || active is true || active?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    private static bool Flag(Dictionary<string, object?> item, string key, bool defaultValue) => !item.TryGetValue(key, out var flag) ? defaultValue : flag is true || flag?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
}
