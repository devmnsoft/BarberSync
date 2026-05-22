namespace BarberSync.Application.DTOs;

public record FranchiseGroupDto(Guid Id, Guid TenantId, string Name, string OwnerName, DateTime CreatedAt);
public record FranchiseUnitDto(Guid Id, Guid TenantId, Guid BranchId, Guid GroupId, string Name, string City, string ManagerName, bool Active);
public record UnitMetricDto(Guid UnitId, string UnitName, decimal Revenue, int Appointments, decimal TicketAverage, int RecurringClients, int Professionals);
public record GoalDto(Guid Id, Guid TenantId, Guid? BranchId, Guid? ProfessionalId, string Name, string Metric, decimal TargetValue, DateOnly StartDate, DateOnly EndDate);
public record GoalProgressDto(Guid GoalId, decimal CurrentValue, decimal ProgressPercent, bool Achieved);
public record ProfessionalPerformanceDto(Guid ProfessionalId, Guid TenantId, Guid BranchId, string ProfessionalName, decimal Revenue, int Appointments, decimal TicketAverage, double Rating, int Cancellations, decimal Commission);
public record CrmClientScoreDto(Guid ClientId, Guid TenantId, Guid BranchId, string ClientName, string Segment, int DaysWithoutVisit, decimal LifetimeValue, int CommercialScore);
public record CampaignDto(Guid Id, Guid TenantId, Guid? BranchId, string Name, string Type, DateOnly StartDate, DateOnly EndDate, string Audience, bool Active);
public record BusinessInsightDto(Guid Id, Guid TenantId, Guid? BranchId, string Type, string Priority, string Description, string ExpectedImpact, string SuggestedAction, bool Executable);
public record FinancialPlanDto(Guid Id, Guid TenantId, Guid BranchId, string MonthRef, decimal ForecastRevenue, decimal ForecastExpenses, decimal FixedCosts, decimal VariableCosts, decimal BreakEvenPoint);
public record ReputationSummaryDto(Guid TenantId, Guid BranchId, double Nps, double AverageRating, int Complaints, int Praises, int LowScoreAlerts);

public record ExecutiveDashboardSummaryDto(
    decimal ForecastRevenue,
    decimal RealizedRevenue,
    decimal MonthlyGoal,
    decimal GoalProgressPercent,
    IReadOnlyList<UnitMetricDto> TopUnits,
    IReadOnlyList<ProfessionalPerformanceDto> TopProfessionals,
    IReadOnlyList<BusinessInsightDto> Insights,
    ReputationSummaryDto Reputation);
