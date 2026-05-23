using BarberSync.Application.DTOs.Strategy;
using BarberSync.Application.Services.Strategy;

namespace BarberSync.Tests;

public class StrategicGrowthServiceTests
{
    [Fact]
    public void Should_Return_Dashboard_Summary_With_Data()
    {
        var sut = new StrategicGrowthService();
        var summary = sut.GetDashboardSummary();
        Assert.True(summary.CompetitorsTracked > 0);
        Assert.True(summary.SuppliersTracked > 0);
    }

    [Fact]
    public void Should_Answer_Pricing_Question()
    {
        var sut = new StrategicGrowthService();
        var answer = sut.AskStrategy(new StrategyQuestionDto("Qual serviço está com preço abaixo do ideal?"));
        Assert.Contains("preço", answer.Answer.ToLowerInvariant());
        Assert.NotEmpty(answer.RecommendedActions);
    }
}
