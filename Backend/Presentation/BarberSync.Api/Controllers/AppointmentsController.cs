using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(EnterpriseDataService data, ILogger<AppointmentsController> logger) : EnterpriseCrudController(data, logger, "appointments")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateAppointment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateAppointment(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteAppointment(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpPost("{id:guid}/confirm")] public Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken) => Change(id, "Confirmed", cancellationToken);
    [HttpPost("{id:guid}/check-in")] public Task<IActionResult> CheckIn(Guid id, CancellationToken cancellationToken) => Change(id, "CheckedIn", cancellationToken);
    [HttpPost("{id:guid}/start")] public Task<IActionResult> Start(Guid id, CancellationToken cancellationToken) => Change(id, "InService", cancellationToken);
    [HttpPost("{id:guid}/finish")] public Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken) => Change(id, "Finished", cancellationToken);
    [HttpPost("{id:guid}/cancel")] public Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) => Change(id, "Cancelled", cancellationToken);
    private Task<IActionResult> Change(Guid id, string status, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.ChangeAppointmentStatusAsync(id, status, cancellationToken), "Status alterado com sucesso.")));
}
