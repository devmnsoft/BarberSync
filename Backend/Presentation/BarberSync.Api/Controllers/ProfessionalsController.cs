using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize]
[Route("api/professionals")]
public sealed class ProfessionalsController(EnterpriseDataService data, ILogger<ProfessionalsController> logger) : EnterpriseCrudController(data, logger, "professionals")
{
    [HttpGet, RequirePermission("Professional.Read")] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}"), RequirePermission("Professional.Read")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost, RequirePermission("Professional.Create")] public Task<IActionResult> CreateProfessional([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}"), RequirePermission("Professional.Update")] public Task<IActionResult> UpdateProfessional(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}"), RequirePermission("Professional.Delete")] public Task<IActionResult> DeleteProfessional(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
}
