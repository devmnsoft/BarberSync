using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController(EnterpriseDataService data, ILogger<ServicesController> logger) : EnterpriseCrudController(data, logger, "services")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateService([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateService(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteService(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
}
