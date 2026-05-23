using BarberSync.Application.Abstractions.Hr;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/hr/professionals")]
public class HrProfessionalsController(IHrProfessionalService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<HrProfessionalDto>> Get([FromQuery] Guid tenantId)
        => Ok(service.GetAll(tenantId));

    [HttpGet("{id:guid}/profile")]
    public ActionResult<HrProfessionalDto> GetById([FromRoute] Guid id, [FromQuery] Guid tenantId)
    {
        var profile = service.GetById(id, tenantId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    public ActionResult<HrProfessionalDto> Create([FromBody] CreateHrProfessionalRequest request)
    {
        var created = service.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id, tenantId = created.TenantId }, created);
    }
}

