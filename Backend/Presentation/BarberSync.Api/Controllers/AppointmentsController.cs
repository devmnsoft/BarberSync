using BarberSync.Api.Security;
using BarberSync.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/appointments")]
public sealed class AppointmentsController(IAppointmentService appointments) : ControllerBase
{
    [HttpGet, RequirePermission("Appointment.Read")]
    public async Task<IActionResult> List([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] Guid? professionalId, [FromQuery] Guid? serviceId, [FromQuery] string? status, [FromQuery] string? origin, CancellationToken ct)
        => Ok(await appointments.ListAsync(new(from,to,professionalId,serviceId,status,origin),ct));

    [HttpGet("{id:guid}"), RequirePermission("Appointment.Read")]
    public async Task<IActionResult> Get(Guid id,CancellationToken ct) => await appointments.GetAsync(id,ct) is { } item ? Ok(item) : NotFound();

    [HttpPost, RequirePermission("Appointment.Create")]
    public async Task<IActionResult> Create(CreateAppointmentRequest request,CancellationToken ct)
    { var result=await appointments.CreateAsync(request,ct); return CreatedAtAction(nameof(Get),new{id=result.Id},result); }

    [HttpPut("{id:guid}"), RequirePermission("Appointment.Update")]
    public Task<AppointmentResponse> Update(Guid id,UpdateAppointmentRequest request,CancellationToken ct) => appointments.UpdateAsync(id,request,ct);

    [HttpPost("{id:guid}/confirm"), RequirePermission("Appointment.Update")] public Task<AppointmentResponse> Confirm(Guid id,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"Confirmed",null,ct);
    [HttpPost("{id:guid}/check-in"), RequirePermission("Attendance.CheckIn")] public Task<AppointmentResponse> CheckIn(Guid id,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"CheckedIn",null,ct);
    [HttpPost("{id:guid}/start"), RequirePermission("Attendance.Start")] public Task<AppointmentResponse> Start(Guid id,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"InService",null,ct);
    [HttpPost("{id:guid}/finish"), RequirePermission("Attendance.Finish")] public Task<AppointmentResponse> Finish(Guid id,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"Finished",null,ct);
    [HttpPost("{id:guid}/cancel"), RequirePermission("Appointment.Cancel")] public Task<AppointmentResponse> Cancel(Guid id,CancelAppointmentRequest request,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"Cancelled",request,ct);
    [HttpPost("{id:guid}/no-show"), RequirePermission("Appointment.Update")] public Task<AppointmentResponse> NoShow(Guid id,CancellationToken ct)=>appointments.ChangeStatusAsync(id,"NoShow",null,ct);
    [HttpPost("{id:guid}/reschedule"), RequirePermission("Appointment.Update")] public Task<AppointmentResponse> Reschedule(Guid id,RescheduleAppointmentRequest request,CancellationToken ct)=>appointments.RescheduleAsync(id,request,ct);
    [HttpGet("availability"), RequirePermission("Appointment.Read")] public Task<bool> Availability([FromQuery] Guid professionalId,[FromQuery] Guid serviceId,[FromQuery] DateTimeOffset start,CancellationToken ct)=>appointments.IsAvailableAsync(new(professionalId,serviceId,start),ct);
    [HttpGet("smart-slots"), RequirePermission("Appointment.Read")] public Task<IReadOnlyList<DateTimeOffset>> SmartSlots([FromQuery] Guid professionalId,[FromQuery] Guid serviceId,[FromQuery] DateOnly date,CancellationToken ct)=>appointments.SmartSlotsAsync(professionalId,serviceId,date,ct);
}
