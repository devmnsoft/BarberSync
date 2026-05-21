using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/advanced-ai")]
public class AdvancedAiController : ControllerBase
{
    private static readonly List<AdvancedAiMetricDto> Metrics = new();

    [HttpPost("recognize-video")]
    public ActionResult<AdvancedAiMetricDto> RecognizeVideo([FromBody] VideoRecognitionRequestDto request)
    {
        var metric = new AdvancedAiMetricDto
        {
            Id = Guid.NewGuid(),
            AppointmentId = request.AppointmentId,
            ProfessionalId = request.ProfessionalId,
            InputType = "video",
            PredictedService = request.DurationSeconds > 1800 ? "Corte + Barba Premium" : "Corte Tradicional",
            Confidence = request.DeviceType.Contains("totem", StringComparison.OrdinalIgnoreCase) ? 0.89m : 0.93m,
            ProcessingTimeMs = 800 + request.DurationSeconds,
            ServiceDurationMinutes = Math.Max(20, request.DurationSeconds / 60),
            EfficiencyAlert = request.DurationSeconds > 3600 ? "tempo acima do esperado" : "ok",
            UpsellSuggestions = new List<string> { "Hidratação", "Pigmentação de Barba", "Combo Sobrancelha" },
            CapturedAtUtc = DateTime.UtcNow
        };

        Metrics.Add(metric);
        return Ok(metric);
    }

    [HttpGet("metrics")]
    public ActionResult<IEnumerable<AdvancedAiMetricDto>> GetMetrics() => Ok(Metrics.OrderByDescending(x => x.CapturedAtUtc));

    [HttpGet("efficiency/{professionalId:guid}")]
    public ActionResult<object> GetEfficiency(Guid professionalId)
    {
        var professionalMetrics = Metrics.Where(x => x.ProfessionalId == professionalId).ToList();
        if (!professionalMetrics.Any()) return NotFound();

        return Ok(new
        {
            ProfessionalId = professionalId,
            AverageDuration = professionalMetrics.Average(x => x.ServiceDurationMinutes),
            AverageConfidence = professionalMetrics.Average(x => x.Confidence),
            TotalServices = professionalMetrics.Count
        });
    }
}
