using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Presentation.Controllers;

[Route("api/appointments")]
[Authorize]
public class AppointmentsController : BaseCrudController
{
    [HttpGet]
    public IActionResult GetAll([FromQuery]int page = 1, [FromQuery]int pageSize = 20, [FromQuery]string? search = null, [FromQuery]string? sortBy = "Name", [FromQuery]string? sortDir = "asc")
        => Ok(Paged(Array.Empty<object>(), 0, page, pageSize));

    [HttpGet("{id:guid}")] public IActionResult GetById(Guid id) => Ok(new { id });
    [HttpPost] public IActionResult Create([FromBody] object dto) => CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, dto);
    [HttpPut("{id:guid}")] public IActionResult Update(Guid id, [FromBody] object dto) => Ok(dto);
    [HttpDelete("{id:guid}")] public IActionResult Delete(Guid id) => NoContent();
}
