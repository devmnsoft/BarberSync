using BarberSync.Application.Abstractions.Innovation;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/innovation")]
public class InnovationController : ControllerBase
{
    private readonly IInnovationOrchestrator _orchestrator;

    public InnovationController(IInnovationOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet("professional-performance/{professionalId:guid}")]
    public ActionResult<InnovationProfessionalPerformanceDto> GetProfessionalPerformance(Guid professionalId)
        => Ok(_orchestrator.GetProfessionalPerformance(professionalId));

    [HttpGet("upsell/{clientId:guid}")]
    public ActionResult<IEnumerable<UpsellRecommendationDto>> GetUpsellRecommendations(Guid clientId)
        => Ok(_orchestrator.GetUpsellRecommendations(clientId));

    [HttpGet("smart-schedule")]
    public ActionResult<SmartScheduleSuggestionDto> GetSmartSchedule([FromQuery] Guid clientId, [FromQuery] Guid serviceId)
        => Ok(_orchestrator.SuggestAppointmentSlot(clientId, serviceId));

    [HttpGet("smart-alerts")]
    public ActionResult<IEnumerable<SmartAlertDto>> GetSmartAlerts()
        => Ok(_orchestrator.GetSmartAlerts());
}
