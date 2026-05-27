using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class StrategicManagementController(StrategicManagementService service) : ControllerBase
{
    [HttpGet("/api/franchise/groups")] public ActionResult<IReadOnlyList<FranchiseGroupDto>> Groups() => Ok(service.GetGroups());
    [HttpPost("/api/franchise/groups")] public ActionResult<FranchiseGroupDto> CreateGroup([FromBody] FranchiseGroupDto dto) => Ok(service.CreateGroup(dto));
    [HttpGet("/api/franchise/units")] public ActionResult<IReadOnlyList<FranchiseUnitDto>> Units([FromQuery] Guid? groupId) => Ok(service.GetUnits(groupId));
    [HttpPost("/api/franchise/units")] public ActionResult<FranchiseUnitDto> CreateUnit([FromBody] FranchiseUnitDto dto) => Ok(service.CreateUnit(dto));
    [HttpGet("/api/franchise/ranking")] public ActionResult<IReadOnlyList<UnitMetricDto>> UnitRanking([FromQuery] Guid tenantId) => Ok(service.UnitRanking(tenantId));

    [HttpGet("/api/goals")] public ActionResult<IReadOnlyList<GoalDto>> Goals([FromQuery] Guid tenantId) => Ok(service.GetGoals(tenantId));
    [HttpPost("/api/goals")] public ActionResult<GoalDto> CreateGoal([FromBody] GoalDto dto) => Ok(service.CreateGoal(dto));
    [HttpPost("/api/goals/progress")] public ActionResult<GoalProgressDto> UpdateGoalProgress([FromBody] GoalProgressDto dto) => Ok(service.UpdateGoalProgress(dto));

    [HttpGet("/api/professionals/ranking")] public ActionResult<IReadOnlyList<ProfessionalPerformanceDto>> ProfessionalRanking([FromQuery] Guid tenantId, [FromQuery] Guid? branchId) => Ok(service.ProfessionalRanking(tenantId, branchId));
    [HttpGet("/api/crm/inactive-clients")] public ActionResult<IReadOnlyList<CrmClientScoreDto>> InactiveClients([FromQuery] Guid tenantId, [FromQuery] int days = 30) => Ok(service.InactiveClients(tenantId, days));
    [HttpGet("/api/crm/vip-clients")] public ActionResult<IReadOnlyList<CrmClientScoreDto>> VipClients([FromQuery] Guid tenantId) => Ok(service.VipClients(tenantId));

    [HttpGet("/api/strategic-management/campaigns")] public ActionResult<IReadOnlyList<CampaignDto>> Campaigns([FromQuery] Guid tenantId) => Ok(service.GetCampaigns(tenantId));
    [HttpPost("/api/strategic-management/campaigns")] public ActionResult<CampaignDto> CreateCampaign([FromBody] CampaignDto dto) => Ok(service.CreateCampaign(dto));

    [HttpGet("/api/insights")] public ActionResult<IReadOnlyList<BusinessInsightDto>> Insights([FromQuery] Guid tenantId) => Ok(service.GetInsights(tenantId));
    [HttpPost("/api/financial-planning")] public ActionResult<FinancialPlanDto> UpsertFinancial([FromBody] FinancialPlanDto dto) => Ok(service.UpsertFinancialPlan(dto));

    [HttpGet("/api/executive-dashboard/summary")] public ActionResult<ExecutiveDashboardSummaryDto> Summary([FromQuery] Guid tenantId) => Ok(service.GetExecutiveDashboard(tenantId));
}
