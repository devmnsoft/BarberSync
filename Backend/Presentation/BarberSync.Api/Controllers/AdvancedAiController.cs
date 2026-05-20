using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/advanced-ai")]
public class AdvancedAiController : ControllerBase
{
    private static readonly List<AdvancedAiMetricDto> Metrics = new();

    [HttpPost("recognize-video")]
    public ActionResult<AdvancedAiMetricDto> RecognizeVideo([FromBody] AdvancedAiMetricDto request)
    {
        request.Id = Guid.NewGuid();
        request.CapturedAtUtc = DateTime.UtcNow;
        request.EfficiencyAlert = request.ServiceDurationMinutes > 70 ? "tempo acima do esperado" : "ok";
        request.UpsellSuggestions = new List<string> { "Hidratação", "Pigmentação de Barba" };
        Metrics.Add(request);
        return Ok(request);
    }

    [HttpGet("metrics")]
    public ActionResult<IEnumerable<AdvancedAiMetricDto>> GetMetrics() => Ok(Metrics.OrderByDescending(x => x.CapturedAtUtc));
}
