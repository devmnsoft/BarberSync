using System.Security.Claims;
using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

/// <summary>Authenticated, ownership-scoped operations used by the customer and professional mobile experiences.</summary>
[ApiController, Authorize, Route("api/mobile")]
public sealed class MobileSelfServiceController(IAppointmentService appointments, EnterpriseDataService data, ICurrentUserContext currentUser, ILogger<MobileSelfServiceController> logger) : ControllerBase
{
    [HttpGet("services")]
    public async Task<IActionResult> Services(CancellationToken ct) => Ok(Envelope((await data.ListAsync("services", ct)).Where(Active)));

    [HttpGet("appointments/availability")]
    public async Task<IActionResult> Availability([FromQuery] Guid professionalId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(Envelope((await appointments.SmartSlotsAsync(professionalId, serviceId, date, ct)).Select(startsAt => new { startsAt })));

    [HttpPost("appointments")]
    public async Task<IActionResult> Create(CreateMobileAppointment request, CancellationToken ct) => await Safe(async () =>
    {
        var result = await appointments.CreateAsync(new(ClientId(), request.ProfessionalId, request.ServiceId, request.StartsAt, "Mobile", request.Notes), ct);
        await Event("Mobile.AppointmentCreated", result.Id, "/Admin/Appointments", "Agendamento criado pelo Mobile", ct);
        return Created($"/api/mobile/appointments/{result.Id}", Envelope(result));
    });

    [HttpPost("appointments/{id:guid}/reschedule")]
    public Task<IActionResult> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken ct) => OwnedClientAppointment(id, async item =>
    {
        var result = await appointments.RescheduleAsync(item.Id, request, ct); await Event("Mobile.AppointmentRescheduled", id, "/Admin/Appointments", "Agendamento reagendado pelo Mobile", ct); return Ok(Envelope(result));
    }, ct);

    [HttpPost("appointments/{id:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid id, CancelAppointmentRequest request, CancellationToken ct) => OwnedClientAppointment(id, async item =>
    {
        if (item.ScheduledStart - DateTimeOffset.UtcNow < TimeSpan.FromHours(2)) return Conflict(Error("O prazo mínimo de cancelamento desta unidade é de 2 horas."));
        var result = await appointments.ChangeStatusAsync(item.Id, "Cancelled", request, ct); await Event("Mobile.AppointmentCancelled", id, "/Admin/Appointments", "Agendamento cancelado pelo Mobile", ct); return Ok(Envelope(result));
    }, ct);

    [HttpGet("client/history")]
    public async Task<IActionResult> History(CancellationToken ct) => Ok(Envelope((await appointments.ListAsync(new(null, null, null, null, null, null), ct)).Where(x => x.ClientId == ClientId())));

    [HttpGet("client/benefits")]
    public async Task<IActionResult> Benefits(CancellationToken ct) => Ok(Envelope(new
    {
        packages = await OwnedRows("packages", "clientId", ClientId(), ct), subscriptions = await OwnedRows("subscriptions", "clientId", ClientId(), ct),
        coupons = await OwnedRows("coupons", "clientId", ClientId(), ct), loyalty = await OwnedRows("loyalty_accounts", "clientId", ClientId(), ct)
    }));

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct) => Ok(Envelope(await OwnedRows("notifications", "userId", currentUser.UserId, ct)));

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<IActionResult> ReadNotification(Guid id, CancellationToken ct)
    {
        var owned = (await OwnedRows("notifications", "userId", currentUser.UserId, ct)).Any(x => Value(x, "id").Equals(id.ToString(), StringComparison.OrdinalIgnoreCase));
        if (!owned) return NotFound(Error("Notificação não encontrada."));
        return Ok(Envelope(await data.UpdateAsync("notifications", id, JsonSerializer.SerializeToElement(new { read = true, readAt = DateTimeOffset.UtcNow }), ct)));
    }

    [HttpGet("professional/day")]
    public async Task<IActionResult> ProfessionalDay(CancellationToken ct) => Ok(Envelope((await appointments.ListAsync(new(DateTimeOffset.UtcNow.Date, DateTimeOffset.UtcNow.Date.AddDays(1), currentUser.UserId, null, null, null), ct))));

    [HttpPost("professional/appointments/{id:guid}/start")]
    public Task<IActionResult> Start(Guid id, CancellationToken ct) => OwnedProfessionalAppointment(id, async item => { var result = await appointments.ChangeStatusAsync(item.Id, "InService", null, ct); await Event("Professional.AttendanceStarted", id, "/Admin/Attendances", "Profissional iniciou atendimento", ct); return Ok(Envelope(result)); }, ct);

    [HttpPost("professional/appointments/{id:guid}/finish")]
    public Task<IActionResult> Finish(Guid id, CancellationToken ct) => OwnedProfessionalAppointment(id, async item => { var result = await appointments.ChangeStatusAsync(item.Id, "Finished", null, ct); await Event("Professional.AttendanceFinished", id, "/Admin/ServiceOrders", "Profissional finalizou atendimento", ct); return Ok(Envelope(result)); }, ct);

    [HttpGet("professional/commissions")]
    public async Task<IActionResult> Commissions(CancellationToken ct) => Ok(Envelope(await OwnedRows("commissions", "professionalId", currentUser.UserId, ct)));

    [HttpGet("professional/blocks")]
    public async Task<IActionResult> Blocks(CancellationToken ct) => Ok(Envelope(await OwnedRows("professional_blocks", "professionalId", currentUser.UserId, ct)));

    [HttpPost("professional/blocks")]
    public async Task<IActionResult> Block(ProfessionalBlockRequest request, CancellationToken ct) => await Safe(async () =>
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Error("O motivo do bloqueio é obrigatório."));
        var end = request.EndsAt ?? request.StartsAt.AddMinutes(30);
        var collision = (await appointments.ListAsync(new(request.StartsAt, end, currentUser.UserId, null, "Confirmed", null), ct)).Any();
        if (collision) return Conflict(Error("O bloqueio colide com um atendimento confirmado."));
        var result = await data.CreateAsync("professional_blocks", JsonSerializer.SerializeToElement(new { professionalId = currentUser.UserId, request.StartsAt, endsAt = end, reason = request.Reason.Trim() }), ct);
        await Event("Professional.ScheduleBlocked", currentUser.UserId, "/Admin/Appointments", "Horário bloqueado pelo profissional", ct); return Ok(Envelope(result));
    });

    private Guid ClientId() => Guid.TryParse(User.FindFirstValue("client_id"), out var id) ? id : currentUser.UserId;
    private async Task<IActionResult> OwnedClientAppointment(Guid id, Func<AppointmentResponse, Task<IActionResult>> action, CancellationToken ct) { var item = await appointments.GetAsync(id, ct); return item is null || item.ClientId != ClientId() ? NotFound(Error("Agendamento não encontrado.")) : await Safe(() => action(item)); }
    private async Task<IActionResult> OwnedProfessionalAppointment(Guid id, Func<AppointmentResponse, Task<IActionResult>> action, CancellationToken ct) { var item = await appointments.GetAsync(id, ct); return item is null || item.ProfessionalId != currentUser.UserId ? NotFound(Error("Atendimento não encontrado.")) : await Safe(() => action(item)); }
    private async Task<IReadOnlyList<Dictionary<string, object?>>> OwnedRows(string resource, string ownerKey, Guid owner, CancellationToken ct) => (await data.ListAsync(resource, ct)).Where(x => Value(x, ownerKey).Equals(owner.ToString(), StringComparison.OrdinalIgnoreCase)).ToArray();
    private async Task Event(string action, Guid entityId, string link, string title, CancellationToken ct) { await data.CreateAsync("audit_logs", JsonSerializer.SerializeToElement(new { action, entityId, userId = currentUser.UserId, occurredAt = DateTimeOffset.UtcNow }), ct); await data.CreateAsync("notifications", JsonSerializer.SerializeToElement(new { type = action, title, link, entityId, active = true }), ct); }
    private async Task<IActionResult> Safe(Func<Task<IActionResult>> action) { try { return await action(); } catch (Exception ex) { logger.LogError(ex, "Falha no autoatendimento Mobile."); return StatusCode(500, Error("Não foi possível concluir a operação.")); } }
    private object Error(string message) => new { success = false, message, traceId = HttpContext.TraceIdentifier };
    private static object Envelope(object? value) => new { success = true, data = value };
    private static string Value(Dictionary<string, object?> row, string key) => row.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
    private static bool Active(Dictionary<string, object?> row) => !row.TryGetValue("isActive", out var value) || value is true || value?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
}

public sealed record CreateMobileAppointment(Guid ProfessionalId, Guid ServiceId, DateTimeOffset StartsAt, string? Notes);
public sealed record ProfessionalBlockRequest(DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string Reason);
