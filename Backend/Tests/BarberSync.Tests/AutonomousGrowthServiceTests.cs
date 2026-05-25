using BarberSync.Application.Services.AutonomousGrowth;

namespace BarberSync.Tests;

public class AutonomousGrowthServiceTests
{
    [Fact]
    public void Should_Approve_And_Execute_Action()
    {
        var service = new AutonomousGrowthService();
        var action = service.GetRecommendedActions().First();

        var approved = service.ApproveAction(action.Id);
        var executed = service.ExecuteAction(action.Id);

        Assert.NotNull(approved);
        Assert.Equal("Approved", approved!.Status);
        Assert.NotNull(executed);
        Assert.Equal("Executed", executed!.Status);
    }
}
