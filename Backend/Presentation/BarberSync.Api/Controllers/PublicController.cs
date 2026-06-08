using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController(EnterpriseDataService data, ILogger<PublicController> logger) : ControllerBase
{
    [HttpGet("services")] public Task<IActionResult> Services(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope((await data.ListAsync("services", cancellationToken)).Where(x => IsActive(x) && Flag(x, "visibleOnPublicWeb", true)), "Serviços públicos carregados.")));
    [HttpGet("professionals")] public Task<IActionResult> Professionals(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope((await data.ListAsync("professionals", cancellationToken)).Where(x => IsActive(x) && Flag(x, "visibleOnPublicWeb", true)), "Profissionais públicos carregados.")));
    [HttpPost("appointments")] public Task<IActionResult> Appointment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.PublicAppointmentAsync(payload, cancellationToken), "Agendamento público criado com sucesso.")));
    [HttpPost("leads")] public Task<IActionResult> Lead([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("public_leads", payload, cancellationToken), "Lead público registrado com sucesso.")));

    private async Task<IActionResult> Safe(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (EnterpriseValidationException ex) { return BadRequest(new { success = false, message = "Existem campos inválidos.", data = (object?)null, errors = ex.Errors }); }
        catch (Exception ex) { logger.LogError(ex, "Erro no PublicController."); return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() }); }
    }
    private static object Envelope(object? data, string message) => new { success = true, message, data, errors = Array.Empty<object>() };
    private static bool IsActive(Dictionary<string, object?> item)
    {
        var active = !item.TryGetValue("isActive", out var isActive) || isActive is true || isActive?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        var status = item.TryGetValue("status", out var value) ? value?.ToString() : null;
        return active && !new[] { "Inactive", "Inativo", "Pausado", "Expirado", "Cancelled", "Cancelado" }.Contains(status ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }
    private static bool Flag(Dictionary<string, object?> item, string key, bool defaultValue)
    {
        var aliases = key switch
        {
            "visibleOnPublicWeb" => new[] { "visibleOnPublicWeb", "site", "publicWeb" },
            "visibleOnKiosk" => new[] { "visibleOnKiosk", "kiosk", "totem" },
            _ => new[] { key }
        };

        foreach (var alias in aliases)
        {
            if (!item.TryGetValue(alias, out var flag)) continue;
            if (flag is bool boolValue) return boolValue;
            var text = flag?.ToString();
            if (string.IsNullOrWhiteSpace(text)) return defaultValue;
            if (text.Equals("Sim", StringComparison.OrdinalIgnoreCase)) return true;
            if (text.Equals("Não", StringComparison.OrdinalIgnoreCase) || text.Equals("Nao", StringComparison.OrdinalIgnoreCase)) return false;
            if (bool.TryParse(text, out var parsed)) return parsed;
        }

        return defaultValue;
    }
}
