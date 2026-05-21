using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/ultra-platform")]
public class UltraAdvancedPlatformController : ControllerBase
{
    [HttpPost("ai/video-recognition")]
    public ActionResult<VideoRecognitionInsightDto> ProcessVideoRecognition([FromBody] VideoRecognitionRequestDto request)
    {
        var recognizedServices = new[]
        {
            new ServiceRecognitionEventDto("Corte Degradê", 0.96m, "Máquina + tesoura", "Finalização com textura"),
            new ServiceRecognitionEventDto("Barba Terapêutica", 0.91m, "Navalha", "Compressa quente")
        };

        return Ok(new VideoRecognitionInsightDto
        {
            AppointmentId = request.AppointmentId,
            ProfessionalId = request.ProfessionalId,
            ProcessedAtUtc = DateTime.UtcNow,
            DurationSeconds = request.DurationSeconds,
            PostureScore = 0.93m,
            InstrumentHandlingScore = 0.95m,
            TechniqueAdherenceScore = 0.92m,
            RecognizedServices = recognizedServices,
            UpsellSuggestions = new[]
            {
                "Adicionar hidratação premium com 15% de desconto",
                "Oferta de combo barba + sobrancelha VIP"
            }
        });
    }

    [HttpGet("automation/triggers")]
    public ActionResult<IEnumerable<AutomationTriggerStateDto>> GetAutomationTriggers()
    {
        return Ok(new[]
        {
            new AutomationTriggerStateDto("overbooking", true, "Redirecionando clientes para horários alternativos"),
            new AutomationTriggerStateDto("low-stock", true, "Compra automática disparada para fornecedor integrado"),
            new AutomationTriggerStateDto("delay-alert", true, "Notificação enviada para cliente e gerente")
        });
    }

    [HttpGet("analytics/predictive")]
    public ActionResult<PredictiveAnalyticsSummaryDto> GetPredictiveAnalytics()
    {
        return Ok(new PredictiveAnalyticsSummaryDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            PeakHours = new[] { "09:00", "12:00", "18:00" },
            TopServices = new[] { "Corte Premium", "Coloração Express", "Barba Terapêutica" },
            StockRiskItems = new[] { "Pomada Matte", "Óleo de Barba" },
            OccupancyProjection = 0.84m,
            ProfitProjection = 0.37m,
            SatisfactionProjection = 0.91m
        });
    }
}
