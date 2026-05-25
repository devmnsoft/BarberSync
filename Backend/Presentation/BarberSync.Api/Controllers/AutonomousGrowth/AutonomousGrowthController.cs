using BarberSync.Application.Abstractions.AutonomousGrowth;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.AutonomousGrowth;

[ApiController]
public class AutonomousGrowthController(IAutonomousGrowthService service) : ControllerBase
{
    [HttpGet("/api/growth-engine/opportunities")]
    public ActionResult<IReadOnlyList<GrowthOpportunityDto>> GetOpportunities() => Ok(service.GetOpportunities());

    [HttpGet("/api/growth-engine/opportunities/{id:guid}")]
    public ActionResult<GrowthOpportunityDto> GetOpportunityById(Guid id)
    {
        var value = service.GetOpportunity(id);
        return value is null ? NotFound() : Ok(value);
    }

    [HttpGet("/api/growth-engine/recommended-actions")]
    public ActionResult<IReadOnlyList<GrowthRecommendedActionDto>> GetRecommendedActions() => Ok(service.GetRecommendedActions());

    [HttpPost("/api/growth-engine/actions/{id:guid}/approve")]
    public ActionResult<GrowthRecommendedActionDto> ApproveAction(Guid id) => ResolveAction(service.ApproveAction(id));

    [HttpPost("/api/growth-engine/actions/{id:guid}/reject")]
    public ActionResult<GrowthRecommendedActionDto> RejectAction(Guid id) => ResolveAction(service.RejectAction(id));

    [HttpPost("/api/growth-engine/actions/{id:guid}/execute")]
    public ActionResult<GrowthRecommendedActionDto> ExecuteAction(Guid id) => ResolveAction(service.ExecuteAction(id));

    [HttpGet("/api/prescriptive-ai/recommendations")]
    public ActionResult<IReadOnlyList<PrescriptiveRecommendationDto>> GetRecommendations() => Ok(service.GetPrescriptiveRecommendations());

    [HttpGet("/api/revenue-management/slots")]
    public ActionResult<IReadOnlyList<RevenueSlotDto>> GetRevenueSlots() => Ok(service.GetRevenueSlots());

    [HttpGet("/api/ab-testing/experiments")]
    public ActionResult<IReadOnlyList<AbExperimentDto>> GetAbExperiments() => Ok(service.GetAbExperiments());

    [HttpGet("/api/retention/risk-scores")]
    public ActionResult<IReadOnlyList<RetentionRiskDto>> GetRetentionRisks() => Ok(service.GetRetentionRisks());

    [HttpGet("/api/autonomous-growth-dashboard/summary")]
    public ActionResult<AutonomousGrowthDashboardSummaryDto> GetDashboardSummary() => Ok(service.GetDashboardSummary());

    private ActionResult<GrowthRecommendedActionDto> ResolveAction(GrowthRecommendedActionDto? action) => action is null ? NotFound() : Ok(action);
}
