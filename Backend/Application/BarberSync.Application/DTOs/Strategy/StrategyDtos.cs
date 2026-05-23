namespace BarberSync.Application.DTOs.Strategy;

public record CompetitorDto(Guid Id, string Name, string Positioning, decimal AvgPrice, decimal AvgRating);
public record DynamicPriceSuggestionDto(Guid ServiceId, string ServiceName, decimal CurrentPrice, decimal SuggestedPrice, string Reason);
public record SupplierProfileDto(Guid Id, string Name, string Category, int LeadTimeDays, decimal Score);
public record PurchaseRequestDto(Guid Id, string ProductName, int Quantity, string Status);
public record ExpansionProjectDto(Guid Id, string Name, string City, decimal Investment, decimal EstimatedPaybackMonths, string Viability);
public record FranchiseHealthDto(Guid BranchId, string BranchName, decimal HealthScore, string Status);
public record BenchmarkRankingDto(Guid BranchId, string BranchName, string Metric, decimal Value, int Rank);
public record StrategicDashboardSummaryDto(
    int CompetitorsTracked,
    int SuppliersTracked,
    int PendingPurchases,
    int PriceAlerts,
    int ExpansionProjects,
    int LowHealthUnits);
public record StrategyQuestionDto(string Question);
public record StrategyAnswerDto(string Answer, IReadOnlyList<string> RecommendedActions);
