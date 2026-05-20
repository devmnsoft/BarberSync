using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
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

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id) => NoContent();
}
