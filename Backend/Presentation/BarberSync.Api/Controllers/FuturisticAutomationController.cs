using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/futuristic-automation")]
public class FuturisticAutomationController : ControllerBase
{
    [HttpGet("operations-snapshot")]
    public ActionResult<FuturisticOperationsSnapshotDto> GetOperationsSnapshot()
    {
        var now = DateTime.UtcNow;

        var forecast = Enumerable.Range(0, 10)
            .Select(index => new DemandForecastPointDto(
                now.AddHours(index),
                index % 2 == 0 ? "Corte Premium" : "Barba Terapêutica",
                8 + index,
                Math.Round(0.62m + (index * 0.03m), 2),
                12 + (index * 2)))
            .ToArray();

        var professionals = new[]
        {
            new FuturisticProfessionalPerformanceDto(Guid.NewGuid(), "Paulo Mendes", 0.94m, 0.89m, 0.96m, 0.31m),
            new FuturisticProfessionalPerformanceDto(Guid.NewGuid(), "Renata Alves", 0.97m, 0.92m, 0.95m, 0.38m),
            new FuturisticProfessionalPerformanceDto(Guid.NewGuid(), "Miguel Rocha", 0.91m, 0.85m, 0.93m, 0.28m)
        };

        var notifications = new[]
        {
            new SmartNotificationEventDto(Guid.NewGuid(), "low-occupancy", "whatsapp", "vip-clients", "Temos horários premium livres hoje às 17h. Ganhe 2x pontos VIP.", now.AddMinutes(5), true),
            new SmartNotificationEventDto(Guid.NewGuid(), "stock-critical", "telegram", "operations", "Estoque crítico de pomada matte. Reposição sugerida em 45 min.", now.AddMinutes(1), false),
            new SmartNotificationEventDto(Guid.NewGuid(), "service-delay", "push", "on-site-clients", "Seu profissional estará disponível em 8 minutos.", now.AddMinutes(2), false)
        };

        var kpis = new Dictionary<string, decimal>
        {
            ["profit_margin"] = 34.7m,
            ["occupancy"] = 81.4m,
            ["nps"] = 89.3m,
            ["prediction_accuracy"] = 92.8m,
            ["automation_coverage"] = 78.1m
        };

        return Ok(new FuturisticOperationsSnapshotDto(now, forecast, professionals, notifications, kpis));
    }

    [HttpGet("bi-export")]
    public ActionResult<BiExportDto> GetBiExport()
    {
        return Ok(new BiExportDto
        {
            Provider = "qlik-sense",
            DatasetName = "barbersync_futuristic_ops",
            GeneratedAtUtc = DateTime.UtcNow,
            Kpis = new Dictionary<string, decimal>
            {
                ["ai_upsell_conversion"] = 27.2m,
                ["cashback_redemption"] = 63.1m,
                ["mfa_adoption"] = 100m
            }
        });
    }
}
