using BarberSync.Application.Abstractions.Strategy;
using BarberSync.Application.DTOs.Strategy;

namespace BarberSync.Application.Services.Strategy;

public class StrategicGrowthService : IStrategicGrowthService
{
    private readonly List<CompetitorDto> _competitors =
    [
        new(Guid.NewGuid(), "Studio Alpha", "Premium", 95, 4.6m),
        new(Guid.NewGuid(), "Corte Rápido", "Econômico", 45, 4.2m)
    ];

    private readonly List<DynamicPriceSuggestionDto> _priceSuggestions =
    [
        new(Guid.NewGuid(), "Corte Premium", 90, 105, "Alta ocupação em horários de pico"),
        new(Guid.NewGuid(), "Barba", 40, 37, "Baixa demanda em dias úteis no período da tarde")
    ];

    private readonly List<SupplierProfileDto> _suppliers =
    [
        new(Guid.NewGuid(), "Fornecedor Prime", "Cosméticos", 2, 9.4m),
        new(Guid.NewGuid(), "Distribuidora Delta", "Descartáveis", 4, 8.8m)
    ];

    private readonly List<PurchaseRequestDto> _purchaseRequests =
    [
        new(Guid.NewGuid(), "Shampoo técnico", 20, "Pendente"),
        new(Guid.NewGuid(), "Toalha descartável", 500, "Aprovado")
    ];

    private readonly List<ExpansionProjectDto> _expansionProjects =
    [
        new(Guid.NewGuid(), "Unidade Zona Sul", "São Paulo", 250000, 18, "Viável"),
        new(Guid.NewGuid(), "Unidade Campinas", "Campinas", 180000, 26, "Atenção")
    ];

    private readonly List<FranchiseHealthDto> _franchiseHealth =
    [
        new(Guid.NewGuid(), "Unidade Jardins", 92, "Acima do padrão"),
        new(Guid.NewGuid(), "Unidade Centro", 68, "Abaixo da média")
    ];

    private readonly List<BenchmarkRankingDto> _benchmarkRankings =
    [
        new(Guid.NewGuid(), "Unidade Jardins", "Margem", 38.5m, 1),
        new(Guid.NewGuid(), "Unidade Centro", "Margem", 22.1m, 5)
    ];

    public IReadOnlyList<CompetitorDto> GetCompetitors() => _competitors;
    public IReadOnlyList<DynamicPriceSuggestionDto> GetPricingSuggestions() => _priceSuggestions;
    public IReadOnlyList<SupplierProfileDto> GetSuppliers() => _suppliers;
    public IReadOnlyList<PurchaseRequestDto> GetPurchaseRequests() => _purchaseRequests;
    public IReadOnlyList<ExpansionProjectDto> GetExpansionProjects() => _expansionProjects;
    public IReadOnlyList<FranchiseHealthDto> GetFranchiseHealth() => _franchiseHealth;
    public IReadOnlyList<BenchmarkRankingDto> GetBenchmarkRankings() => _benchmarkRankings;

    public StrategicDashboardSummaryDto GetDashboardSummary() => new(
        _competitors.Count,
        _suppliers.Count,
        _purchaseRequests.Count(x => x.Status == "Pendente"),
        _priceSuggestions.Count(x => x.SuggestedPrice != x.CurrentPrice),
        _expansionProjects.Count,
        _franchiseHealth.Count(x => x.HealthScore < 70));

    public StrategyAnswerDto AskStrategy(StrategyQuestionDto question)
    {
        var normalized = question.Question.ToLowerInvariant();
        if (normalized.Contains("preço"))
        {
            return new("Existem serviços com preço abaixo do ideal em horários ociosos.",
            ["Ajustar preço da Barba para R$ 37 em horários de baixa demanda", "Testar pacote premium para Corte Premium"]);
        }

        if (normalized.Contains("fornecedor") || normalized.Contains("comprar"))
        {
            return new("O fornecedor com melhor custo-benefício atual é o Fornecedor Prime.",
            ["Priorizar cotação com Fornecedor Prime", "Negociar prazo com Distribuidora Delta"]);
        }

        return new("Há oportunidade de crescimento com expansão controlada e gestão de margem.",
            ["Revisar unidades com health score abaixo de 70", "Executar plano piloto de expansão na Zona Sul"]);
    }
}
