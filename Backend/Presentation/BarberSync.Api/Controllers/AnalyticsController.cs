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
        return Ok(new KpiSnapshotDto
        {
            SnapshotAtUtc = DateTime.UtcNow,
            OccupancyRate = 0.81m,
            Revenue = 124530.90m,
            Profit = 38210.12m,
            SatisfactionScore = 4.8m,
            CriticalStockItems = 3,
            OverloadedProfessionals = 2
        });
    }

    [HttpGet("bi-export")]
    public ActionResult<BiExportDto> ExportBi([FromQuery] string provider = "power-bi")
    {
        return Ok(new BiExportDto
        {
            Provider = provider,
            Kpis = new Dictionary<string, decimal>
            {
                ["occupancy_rate"] = 0.81m,
                ["avg_ticket"] = 92.4m,
                ["low_stock_alerts"] = 3,
                ["demand_prediction"] = 1.18m
            }
        });
    }
}
