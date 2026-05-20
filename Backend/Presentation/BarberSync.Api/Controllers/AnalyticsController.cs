using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("kpis")]
    public ActionResult<KpiSnapshotDto> GetKpis()
    {
        var snapshot = new KpiSnapshotDto
        {
            Revenue = 18250m,
            Profit = 12400m,
            OccupancyRate = 0.86m,
            SatisfactionScore = 4.8m,
            CriticalStockItems = 2,
            OverloadedProfessionals = 1
        };

        return Ok(snapshot);
    }

    [HttpGet("export/powerbi")]
    public IActionResult ExportPowerBi() => Ok(new { endpoint = "/api/analytics/kpis", format = "json" });
}
