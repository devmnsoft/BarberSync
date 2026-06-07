using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/professionals")]
public sealed class ProfessionalsController(EnterpriseDataService data, ILogger<ProfessionalsController> logger) : EnterpriseCrudController(data, logger, "professionals")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateProfessional([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateProfessional(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteProfessional(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
}
