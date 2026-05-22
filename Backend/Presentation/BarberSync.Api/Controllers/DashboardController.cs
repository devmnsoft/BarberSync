using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    [HttpGet("summary")]
    public ActionResult<DashboardSummaryDto> Summary() => Ok(new DashboardSummaryDto(1840, 28400, 92, 74, 18, 52, 12, 46, 4, 81, 3, 420, 1900, 11, 7, "10:00-12:00"));

    [HttpGet("revenue")] public IActionResult Revenue() => Ok(new { labels = new[] { "Seg", "Ter", "Qua", "Qui", "Sex" }, values = new[] { 1200, 1800, 2100, 1700, 2500 } });
    [HttpGet("appointments")] public IActionResult Appointments() => Ok(new { confirmed = 40, canceled = 5, done = 29 });
    [HttpGet("professionals")] public IActionResult Professionals() => Ok(new { top = new[] { "Alex", "Maya", "Rafa" } });
    [HttpGet("stock")] public IActionResult Stock() => Ok(new { critical = new[] { "Pomada X", "Shampoo Y" } });
    [HttpGet("loyalty")] public IActionResult Loyalty() => Ok(new { cashback = 420, pointsIssued = 9200 });
    [HttpGet("ai")] public IActionResult Ai() => Ok(new { detectedServices = 11, correctionsRequested = 2 });
}
