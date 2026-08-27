using BarberSync.Api.Security;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

/// <summary>Canonical scheduling facade. Business transitions remain centralized in IAppointmentService.</summary>
[ApiController, Authorize, Route("api/scheduling")]
public sealed class SchedulingController(IAppointmentService appointments, EnterpriseDataService data) : ControllerBase
{
    [HttpGet("dashboard"), RequirePermission("Scheduling.Read")]
    public async Task<IActionResult> Dashboard([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var items = await appointments.ListAsync(new(from, from.AddDays(1), null, null, null, null), ct);
        return Ok(new { date = day, scheduled = items.Count, confirmed = items.Count(x => x.Status == "Confirmed"), awaitingConfirmation = items.Count(x => x.Status == "Scheduled"), checkIns = items.Count(x => x.Status == "CheckedIn"), noShows = items.Count(x => x.Status == "NoShow"), professionalsAvailable = items.Select(x => x.ProfessionalId).Distinct().Count(), sourceStatus = "Operational" });
    }

    [HttpGet("calendar"), RequirePermission("Scheduling.Read")]
    public Task<IReadOnlyList<AppointmentResponse>> Calendar([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] Guid? professionalId, [FromQuery] Guid? serviceId, [FromQuery] string? status, CancellationToken ct)
        => appointments.ListAsync(new(from, to, professionalId, serviceId, status, null), ct);

    [HttpGet("availability"), RequirePermission("Scheduling.Read")]
    public Task<IReadOnlyList<DateTimeOffset>> Availability([FromQuery] Guid professionalId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken ct)
        => appointments.SmartSlotsAsync(professionalId, serviceId, date, ct);

    [HttpPost("appointments"), RequirePermission("Scheduling.Book")]
    public async Task<IActionResult> Create(CreateAppointmentRequest request, CancellationToken ct)
    {
        var created = await appointments.CreateAsync(request, ct);
        return Created($"/api/scheduling/appointments/{created.Id}", created);
    }

    [HttpPost("appointments/{id:guid}/reschedule"), RequirePermission("Scheduling.Reschedule")]
    public Task<AppointmentResponse> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken ct) => appointments.RescheduleAsync(id, request, ct);
    [HttpPost("appointments/{id:guid}/cancel"), RequirePermission("Scheduling.Cancel")]
    public Task<AppointmentResponse> Cancel(Guid id, CancelAppointmentRequest request, CancellationToken ct) => appointments.ChangeStatusAsync(id, "Cancelled", request.Reason, ct);
    [HttpPost("appointments/{id:guid}/confirm"), RequirePermission("Scheduling.Manage")]
    public Task<AppointmentResponse> Confirm(Guid id, CancellationToken ct) => appointments.ChangeStatusAsync(id, "Confirmed", null, ct);
    [HttpPost("appointments/{id:guid}/check-in"), RequirePermission("Scheduling.Manage")]
    public Task<AppointmentResponse> CheckIn(Guid id, CancellationToken ct) => appointments.ChangeStatusAsync(id, "CheckedIn", null, ct);
    [HttpPost("appointments/{id:guid}/no-show"), RequirePermission("Scheduling.Manage")]
    public Task<AppointmentResponse> NoShow(Guid id, NoShowAppointmentRequest request, CancellationToken ct) => appointments.ChangeStatusAsync(id, "NoShow", request.Reason, ct);

    [HttpGet("filter-options"), RequirePermission("Scheduling.Read")]
    public async Task<IActionResult> FilterOptions(CancellationToken ct)
    {
        var services = await data.ListAsync("services", ct);
        var professionals = await data.ListAsync("professionals", ct);
        var clients = await data.ListAsync("clients", ct);
        return Ok(new { clients, services, professionals, statuses = new[] { "Scheduled", "Confirmed", "CheckedIn", "InService", "Finished", "Cancelled", "NoShow" } });
    }
}
