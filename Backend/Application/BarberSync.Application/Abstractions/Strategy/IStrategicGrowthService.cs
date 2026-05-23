using BarberSync.Application.DTOs.Strategy;

namespace BarberSync.Application.Abstractions.Strategy;

public interface IStrategicGrowthService
{
    IReadOnlyList<CompetitorDto> GetCompetitors();
    IReadOnlyList<DynamicPriceSuggestionDto> GetPricingSuggestions();
    IReadOnlyList<SupplierProfileDto> GetSuppliers();
    IReadOnlyList<PurchaseRequestDto> GetPurchaseRequests();
    IReadOnlyList<ExpansionProjectDto> GetExpansionProjects();
    IReadOnlyList<FranchiseHealthDto> GetFranchiseHealth();
    IReadOnlyList<BenchmarkRankingDto> GetBenchmarkRankings();
    StrategicDashboardSummaryDto GetDashboardSummary();
    StrategyAnswerDto AskStrategy(StrategyQuestionDto question);
}
