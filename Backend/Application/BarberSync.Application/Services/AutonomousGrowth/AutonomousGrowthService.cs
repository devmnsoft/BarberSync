using BarberSync.Application.Abstractions.AutonomousGrowth;
using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.AutonomousGrowth;

public class AutonomousGrowthService : IAutonomousGrowthService
{
    private readonly List<GrowthOpportunityDto> _opportunities =
    [
        new(Guid.NewGuid(), "tenant-demo", "branch-matriz", "HORARIO_OCIOSO", "Baixa ocupação terça 14h-17h", 480m, "Alta", "Open"),
        new(Guid.NewGuid(), "tenant-demo", "branch-matriz", "CLIENTE_INATIVO", "Clientes inativos há mais de 45 dias", 920m, "Média", "Open")
    ];

    private readonly List<GrowthRecommendedActionDto> _actions;

    public AutonomousGrowthService()
    {
        _actions =
        [
            new(Guid.NewGuid(), _opportunities[0].Id, "CAMPAIGN", "Campanha 20% barba terça 14h-17h para base inativa", 480m, true, "PendingApproval"),
            new(Guid.NewGuid(), _opportunities[1].Id, "RETENTION", "Fluxo de reativação com cashback de retorno", 920m, true, "PendingApproval")
        ];
    }

    public IReadOnlyList<GrowthOpportunityDto> GetOpportunities() => _opportunities;
    public GrowthOpportunityDto? GetOpportunity(Guid id) => _opportunities.FirstOrDefault(x => x.Id == id);
    public IReadOnlyList<GrowthRecommendedActionDto> GetRecommendedActions() => _actions;

    public GrowthRecommendedActionDto? ApproveAction(Guid id) => UpdateActionStatus(id, "Approved");
    public GrowthRecommendedActionDto? RejectAction(Guid id) => UpdateActionStatus(id, "Rejected");
    public GrowthRecommendedActionDto? ExecuteAction(Guid id) => UpdateActionStatus(id, "Executed");

    public IReadOnlyList<PrescriptiveRecommendationDto> GetPrescriptiveRecommendations() =>
    [
        new(Guid.NewGuid(), "Baixa ocupação terça-feira à tarde", "Falta de oferta específica para janela ociosa", "Campanha segmentada com incentivo e escassez", 0.87m, 120m, 480m, "Agendamentos confirmados")
    ];

    public IReadOnlyList<RevenueSlotDto> GetRevenueSlots() =>
    [
        new(Guid.NewGuid(), "Tuesday", "14:00-17:00", 260m, 480m, 0.43m, 37m),
        new(Guid.NewGuid(), "Friday", "18:00-20:00", 990m, 0m, 0.95m, 59m)
    ];

    public IReadOnlyList<AbExperimentDto> GetAbExperiments() =>
    [
        new(Guid.NewGuid(), "Mensagem WhatsApp Reativação", "Mensagem com urgência converte mais", "conversion_rate", DateTime.UtcNow.AddDays(-5), null, "Running")
    ];

    public IReadOnlyList<RetentionRiskDto> GetRetentionRisks() =>
    [
        new(Guid.NewGuid(), "VIP_EM_RISCO", 0.79m, "Contato com profissional favorito + voucher premium")
    ];

    public AutonomousGrowthDashboardSummaryDto GetDashboardSummary() => new(
        _opportunities.Count(x => x.Status == "Open"),
        _actions.Count(x => x.Status == "PendingApproval"),
        _actions.Count(x => x.Status == "Executed"),
        _actions.Where(x => x.Status == "Executed").Sum(x => x.EstimatedRevenueImpact),
        _actions.Any(x => x.Status == "Executed") ? 1.42m : 0m,
        17);

    private GrowthRecommendedActionDto? UpdateActionStatus(Guid id, string status)
    {
        var index = _actions.FindIndex(x => x.Id == id);
        if (index < 0) return null;

        var current = _actions[index];
        var updated = current with { Status = status };
        _actions[index] = updated;
        return updated;
    }
}
