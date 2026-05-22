using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;

namespace BarberSync.Tests;

public class StrategicManagementServiceTests
{
    [Fact]
    public void FranchiseServiceTests_Should_Create_Unit()
    {
        var service = new StrategicManagementService(new InMemorySaasStore());
        var group = service.GetGroups().First();
        var unit = service.CreateUnit(new FranchiseUnitDto(Guid.NewGuid(), group.TenantId, Guid.NewGuid(), group.Id, "Nova Unidade", "Campinas", "Leo", true));
        Assert.Contains(service.GetUnits(group.Id), x => x.Id == unit.Id);
    }

    [Fact]
    public void GoalServiceTests_Should_Update_Progress()
    {
        var service = new StrategicManagementService(new InMemorySaasStore());
        var goal = service.GetGoals(service.GetGroups().First().TenantId).First();
        var progress = service.UpdateGoalProgress(new GoalProgressDto(goal.Id, 50000, 100, true));
        Assert.True(progress.Achieved);
    }

    [Fact]
    public void ExecutiveDashboardServiceTests_Should_Return_Summary()
    {
        var service = new StrategicManagementService(new InMemorySaasStore());
        var tenantId = service.GetGroups().First().TenantId;
        var summary = service.GetExecutiveDashboard(tenantId);
        Assert.NotEmpty(summary.TopUnits);
    }
}
