using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/audit")]
public sealed class AuditController(EnterpriseDataService data, ILogger<AuditController> logger) : EnterpriseCrudController(data, logger, "audit_logs")
{
    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);

    [HttpGet("events")]
    public Task<IActionResult> Events(CancellationToken cancellationToken) => List(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
}
