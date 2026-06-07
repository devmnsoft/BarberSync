using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController(EnterpriseDataService data, ILogger<ClientsController> logger) : EnterpriseCrudController(data, logger, "clients")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateClient([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateClient(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
}
