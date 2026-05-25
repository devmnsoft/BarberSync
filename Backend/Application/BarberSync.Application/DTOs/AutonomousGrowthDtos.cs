namespace BarberSync.Application.DTOs;

public record GrowthOpportunityDto(Guid Id, string TenantId, string BranchId, string Type, string Title, decimal ExpectedImpact, string Priority, string Status);
public record GrowthRecommendedActionDto(Guid Id, Guid OpportunityId, string ActionType, string Description, decimal EstimatedRevenueImpact, bool RequiresApproval, string Status);
public record PrescriptiveRecommendationDto(Guid Id, string Diagnosis, string ProbableCause, string RecommendedAction, decimal Confidence, decimal EstimatedCost, decimal EstimatedImpact, string SuccessMetric);
public record RevenueSlotDto(Guid Id, string DayOfWeek, string TimeRange, decimal Revenue, decimal LostRevenue, decimal OccupancyRate, decimal SuggestedPrice);
public record AbExperimentDto(Guid Id, string Name, string Hypothesis, string PrimaryMetric, DateTime StartAt, DateTime? EndAt, string Status);
public record RetentionRiskDto(Guid ClientId, string Segment, decimal RiskScore, string SuggestedAction);
public record AutonomousGrowthDashboardSummaryDto(int OpenOpportunities, int PendingApprovals, int ExecutedActions, decimal IncrementalRevenue, decimal AverageRoi, int RecoveredClients);
