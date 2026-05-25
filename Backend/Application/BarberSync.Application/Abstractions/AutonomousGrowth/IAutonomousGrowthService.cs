using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.AutonomousGrowth;

public interface IAutonomousGrowthService
{
    IReadOnlyList<GrowthOpportunityDto> GetOpportunities();
    GrowthOpportunityDto? GetOpportunity(Guid id);
    IReadOnlyList<GrowthRecommendedActionDto> GetRecommendedActions();
    GrowthRecommendedActionDto? ApproveAction(Guid id);
    GrowthRecommendedActionDto? RejectAction(Guid id);
    GrowthRecommendedActionDto? ExecuteAction(Guid id);
    IReadOnlyList<PrescriptiveRecommendationDto> GetPrescriptiveRecommendations();
    IReadOnlyList<RevenueSlotDto> GetRevenueSlots();
    IReadOnlyList<AbExperimentDto> GetAbExperiments();
    IReadOnlyList<RetentionRiskDto> GetRetentionRisks();
    AutonomousGrowthDashboardSummaryDto GetDashboardSummary();
}
