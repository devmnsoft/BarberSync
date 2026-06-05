using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc", [FromQuery] string? search = null)
        => Ok(new { page, pageSize, sortBy, sortOrder, search, items = Array.Empty<object>() });

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => Ok(new { id });

    [HttpPost]
    public IActionResult Create([FromBody] object request) => CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, request);

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] object request) => Ok(request);

    [HttpPost("{id}/confirm")]
    public IActionResult Confirm(string id) => Ok(new { success = true, message = "Agendamento confirmado com sucesso.", data = new { id, status = "Confirmed" }, isDemo = true });

    [HttpPost("{id}/check-in")]
    public IActionResult CheckIn(string id) => Ok(new { success = true, message = "Check-in realizado com sucesso.", data = new { id, status = "CheckedIn" }, isDemo = true });

    [HttpPost("{id}/start")]
    public IActionResult Start(string id) => Ok(new { success = true, message = "Atendimento iniciado com sucesso.", data = new { id, status = "InService" }, isDemo = true });

    [HttpPost("{id}/finish")]
    public IActionResult Finish(string id) => Ok(new { success = true, message = "Atendimento finalizado com sucesso.", data = new { id, status = "Finished" }, isDemo = true });

    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(string id) => Ok(new { success = true, message = "Agendamento cancelado com sucesso.", data = new { id, status = "Canceled" }, isDemo = true });

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id) => NoContent();
}
