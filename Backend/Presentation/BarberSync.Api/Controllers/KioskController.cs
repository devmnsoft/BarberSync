using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarberSync.Application.Operations;

namespace BarberSync.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/kiosk")]
public sealed class KioskController(EnterpriseDataService data, IAppointmentService appointments, IServiceOrderService orders, IConfiguration configuration, ILogger<KioskController> logger) : ControllerBase
{
    [HttpGet("branches")] public Task<IActionResult> Branches([FromQuery] string deviceCode, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await data.ListAsync("branches", cancellationToken)).Where(IsActive), "Unidades ativas carregadas.")), cancellationToken);
    [HttpGet("status")] public Task<IActionResult> Status(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.KioskStatusAsync(cancellationToken), "Status do totem carregado com dados reais.")));
    [HttpGet("services")] public Task<IActionResult> Services([FromQuery] string deviceCode, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await data.ListAsync("services", cancellationToken)).Where(x => IsActive(x) && Flag(x, "visibleOnKiosk", true)), "Serviços do totem carregados.")), cancellationToken);
    [HttpGet("professionals")] public Task<IActionResult> Professionals([FromQuery] string deviceCode, [FromQuery] string? serviceId, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await data.ListAsync("professionals", cancellationToken)).Where(IsActive), "Profissionais do totem carregados.")), cancellationToken);
    [HttpGet("availability")] public Task<IActionResult> Availability([FromQuery] string deviceCode, [FromQuery] Guid professionalId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken cancellationToken) => WithDevice(deviceCode, async () => Ok(Envelope((await appointments.SmartSlotsAsync(professionalId, serviceId, date, cancellationToken)).Select(startsAt => new { startsAt }), "Horários disponíveis carregados com escala, pausas e bloqueios.")), cancellationToken);
    [HttpPost("session")] public Task<IActionResult> Session([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("kiosk_sessions", payload, cancellationToken), "Atendimento enviado ao caixa.")));
    [HttpPost("payment")]
    public Task<IActionResult> Payment([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("Payments:Enabled") || string.IsNullOrWhiteSpace(configuration["Payments:Provider"]))
        {
            return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                success = false,
                message = "Pagamento no totem indisponível. Envie a comanda para o caixa ou chame um atendente.",
                data = (object?)null,
                errors = new[] { new { field = "payment", message = "Nenhum provedor de pagamento está configurado." } }
            }));
        }
        return Safe(async () => Ok(Envelope(await data.CreateAsync("payments", payload, cancellationToken), "Pagamento confirmado pelo provedor configurado.")));
    }
    [HttpPost("client/quick-register")] public Task<IActionResult> QuickClient([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("clients", payload, cancellationToken), "Cliente rápido cadastrado.")));
    [HttpPost("client/find-by-phone")] public Task<IActionResult> FindClientByPhone([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () =>
    {
        var phone = payload.TryGetProperty("phone", out var value) ? Digits(value.GetString()) : string.Empty;
        if (phone.Length < 10) throw new EnterpriseValidationException([new("phone", "Informe um telefone válido com DDD.")]);
        var client = (await data.ListAsync("clients", cancellationToken)).FirstOrDefault(item =>
            item.TryGetValue("phone", out var candidate) && Digits(candidate?.ToString()) == phone);
        return Ok(Envelope(client, client is null ? "Cliente não encontrado." : "Cliente localizado."));
    });
    [HttpPost("check-in")] public Task<IActionResult> CheckIn([FromBody] KioskCheckInRequest request, CancellationToken cancellationToken) => Safe(async () =>
    {
        if (string.IsNullOrWhiteSpace(request.DeviceCode)) throw new EnterpriseValidationException([new("deviceCode", "DeviceCode obrigatório.")]);
        var appointment = await appointments.CreateAsync(new(request.ClientId, request.ProfessionalId, request.ServiceId, request.StartsAt, "Kiosk", "Check-in criado por autoatendimento."), cancellationToken);
        appointment = await appointments.ChangeStatusAsync(appointment.Id, "CheckedIn", null, cancellationToken);
        await data.CreateAsync("audit_logs", JsonSerializer.SerializeToElement(new { action = "Kiosk.CheckIn", entityId = appointment.Id, origin = "Kiosk", occurredAt = DateTimeOffset.UtcNow }), cancellationToken);
        await data.CreateAsync("notifications", JsonSerializer.SerializeToElement(new { type = "KioskCheckIn", title = "Novo check-in do Totem", link = "/Admin/Attendances", entityId = appointment.Id, active = true }), cancellationToken);
        return Ok(Envelope(new { appointmentId = appointment.Id, ticket = appointment.Id.ToString("N")[..6].ToUpperInvariant(), appointment.Status }, "Check-in confirmado e enviado ao Atendimento."));
    });
    [HttpPost("pre-orders")] public Task<IActionResult> PreOrder([FromBody] KioskPreOrderRequest request, CancellationToken cancellationToken) => Safe(async () =>
    {
        if (string.IsNullOrWhiteSpace(request.DeviceCode)) throw new EnterpriseValidationException([new("deviceCode", "DeviceCode obrigatório.")]);
        var order = await orders.OpenAsync(new(request.ClientId, request.AppointmentId, "Pré-comanda criada no Totem."), cancellationToken);
        order = await orders.AddServiceAsync(order.Id, new(request.ServiceId, request.ProfessionalId), cancellationToken);
        await data.CreateAsync("audit_logs", JsonSerializer.SerializeToElement(new { action = "Kiosk.PreOrderCreated", entityId = order.Id, origin = "Kiosk", occurredAt = DateTimeOffset.UtcNow }), cancellationToken);
        await data.CreateAsync("notifications", JsonSerializer.SerializeToElement(new { type = "KioskPreOrder", title = "Pré-comanda aguardando atendimento", link = $"/Admin/ServiceOrders?order={order.Id}", entityId = order.Id, active = true }), cancellationToken);
        return Ok(Envelope(new { orderId = order.Id, order.Number, order.Status }, "Pré-comanda enviada ao PDV."));
    });
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
        catch (Exception ex) { logger.LogError(ex, "Erro no KioskController."); return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", traceId = HttpContext.TraceIdentifier, data = (object?)null, errors = Array.Empty<object>() }); }
    }
    private static object Envelope(object? data, string message) => new { success = true, message, data, errors = Array.Empty<object>() };
    private static string Digits(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());
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

public sealed record KioskCheckInRequest(string DeviceCode, Guid BranchId, Guid ClientId, Guid ServiceId, Guid ProfessionalId, DateTimeOffset StartsAt, string Origin);
public sealed record KioskPreOrderRequest(string DeviceCode, Guid BranchId, Guid ClientId, Guid ServiceId, Guid ProfessionalId, Guid AppointmentId, DateTimeOffset StartsAt, string Origin);
