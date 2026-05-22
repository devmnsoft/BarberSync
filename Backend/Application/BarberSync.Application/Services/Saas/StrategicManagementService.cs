using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class StrategicManagementService
{
    private readonly InMemorySaasStore _store;
    public StrategicManagementService(InMemorySaasStore store)
    {
        _store = store;
        if (_store.FranchiseGroups.Count == 0) Seed();
    }

    public IReadOnlyList<FranchiseGroupDto> GetGroups() => _store.FranchiseGroups;
    public FranchiseGroupDto CreateGroup(FranchiseGroupDto dto) { _store.FranchiseGroups.Add(dto); return dto; }
    public IReadOnlyList<FranchiseUnitDto> GetUnits(Guid? groupId = null) => _store.FranchiseUnits.Where(x => groupId is null || x.GroupId == groupId).ToList();
    public FranchiseUnitDto CreateUnit(FranchiseUnitDto dto) { _store.FranchiseUnits.Add(dto); return dto; }
    public IReadOnlyList<UnitMetricDto> UnitRanking(Guid tenantId) => _store.UnitMetrics.Where(x => _store.FranchiseUnits.Any(u => u.Id == x.UnitId && u.TenantId == tenantId)).OrderByDescending(x => x.Revenue).ToList();

    public GoalDto CreateGoal(GoalDto goal) { _store.Goals.Add(goal); return goal; }
    public IReadOnlyList<GoalDto> GetGoals(Guid tenantId) => _store.Goals.Where(g => g.TenantId == tenantId).ToList();
    public GoalProgressDto UpdateGoalProgress(GoalProgressDto progress)
    {
        var existing = _store.GoalProgress.FirstOrDefault(x => x.GoalId == progress.GoalId);
        if (existing is not null) _store.GoalProgress.Remove(existing);
        _store.GoalProgress.Add(progress);
        return progress;
    }

    public IReadOnlyList<ProfessionalPerformanceDto> ProfessionalRanking(Guid tenantId, Guid? branchId = null)
        => _store.ProfessionalPerformance
            .Where(x => x.TenantId == tenantId && (branchId is null || x.BranchId == branchId))
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.Rating)
            .ToList();

    public IReadOnlyList<CrmClientScoreDto> InactiveClients(Guid tenantId, int days = 30)
        => _store.ClientScores.Where(x => x.TenantId == tenantId && x.DaysWithoutVisit >= days).OrderByDescending(x => x.DaysWithoutVisit).ToList();
    public IReadOnlyList<CrmClientScoreDto> VipClients(Guid tenantId)
        => _store.ClientScores.Where(x => x.TenantId == tenantId && x.Segment == "VIP").OrderByDescending(x => x.LifetimeValue).ToList();

    public CampaignDto CreateCampaign(CampaignDto dto) { _store.Campaigns.Add(dto); return dto; }
    public IReadOnlyList<CampaignDto> GetCampaigns(Guid tenantId) => _store.Campaigns.Where(x => x.TenantId == tenantId).ToList();

    public IReadOnlyList<BusinessInsightDto> GetInsights(Guid tenantId) => _store.BusinessInsights.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.Priority).ToList();
    public FinancialPlanDto UpsertFinancialPlan(FinancialPlanDto dto)
    {
        var existing = _store.FinancialPlans.FirstOrDefault(x => x.TenantId == dto.TenantId && x.BranchId == dto.BranchId && x.MonthRef == dto.MonthRef);
        if (existing is not null) _store.FinancialPlans.Remove(existing);
        _store.FinancialPlans.Add(dto);
        return dto;
    }

    public ExecutiveDashboardSummaryDto GetExecutiveDashboard(Guid tenantId)
    {
        var ranking = UnitRanking(tenantId).Take(5).ToList();
        var pros = ProfessionalRanking(tenantId).Take(5).ToList();
        var insights = GetInsights(tenantId).Take(5).ToList();
        var goals = _store.Goals.Where(x => x.TenantId == tenantId).ToList();
        var progress = _store.GoalProgress.ToDictionary(x => x.GoalId, x => x.ProgressPercent);
        var monthlyGoal = goals.Sum(x => x.TargetValue);
        var goalProgress = goals.Count == 0 ? 0 : goals.Average(x => progress.GetValueOrDefault(x.Id, 0));
        var forecastRevenue = _store.FinancialPlans.Where(x => x.TenantId == tenantId).Sum(x => x.ForecastRevenue);
        var realizedRevenue = ranking.Sum(x => x.Revenue);
        var rep = _store.ReputationSummaries.First(x => x.TenantId == tenantId);
        return new ExecutiveDashboardSummaryDto(forecastRevenue, realizedRevenue, monthlyGoal, goalProgress, ranking, pros, insights, rep);
    }

    private void Seed()
    {
        var tenant = Guid.NewGuid();
        var group = new FranchiseGroupDto(Guid.NewGuid(), tenant, "Rede BarberSync Prime", "Grupo Prime", DateTime.UtcNow);
        var b1 = Guid.NewGuid(); var b2 = Guid.NewGuid();
        var u1 = new FranchiseUnitDto(Guid.NewGuid(), tenant, b1, group.Id, "Unidade Centro", "São Paulo", "Julia", true);
        var u2 = new FranchiseUnitDto(Guid.NewGuid(), tenant, b2, group.Id, "Unidade Jardins", "São Paulo", "Marcos", true);
        _store.FranchiseGroups.Add(group); _store.FranchiseUnits.AddRange([u1, u2]);
        _store.UnitMetrics.AddRange([new(u1.Id, u1.Name, 52000, 480, 108, 220, 9), new(u2.Id, u2.Name, 47000, 430, 109, 198, 8)]);
        var p1 = new ProfessionalPerformanceDto(Guid.NewGuid(), tenant, b1, "Rafa", 18500, 170, 108, 4.9, 2, 3700);
        var p2 = new ProfessionalPerformanceDto(Guid.NewGuid(), tenant, b2, "Nina", 16200, 150, 108, 4.8, 3, 3240);
        _store.ProfessionalPerformance.AddRange([p1, p2]);
        _store.ClientScores.AddRange([new(Guid.NewGuid(), tenant, b1, "Carlos", "VIP", 12, 4200, 95), new(Guid.NewGuid(), tenant, b1, "Marta", "Inativo", 45, 890, 62)]);
        _store.Campaigns.Add(new CampaignDto(Guid.NewGuid(), tenant, b1, "Volta Cliente 30+", "DESCONTO_PERCENTUAL", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), "Inativos", true));
        _store.BusinessInsights.Add(new BusinessInsightDto(Guid.NewGuid(), tenant, b1, "LOW_OCCUPANCY", "ALTA", "Horário 14h-16h com baixa ocupação.", "+12% agendamentos na faixa", "Criar promoção de horário ocioso", true));
        _store.FinancialPlans.Add(new FinancialPlanDto(Guid.NewGuid(), tenant, b1, DateTime.UtcNow.ToString("yyyy-MM"), 65000, 35000, 18000, 17000, 32000));
        _store.ReputationSummaries.Add(new ReputationSummaryDto(tenant, b1, 76, 4.7, 3, 42, 1));
        var goal = new GoalDto(Guid.NewGuid(), tenant, b1, p1.ProfessionalId, "Meta Receita Mensal", "RECEITA_MENSAL", 50000, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25)));
        _store.Goals.Add(goal);
        _store.GoalProgress.Add(new GoalProgressDto(goal.Id, 27500, 55, false));
    }
}
