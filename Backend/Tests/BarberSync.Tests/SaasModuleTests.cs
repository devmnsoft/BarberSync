using BarberSync.Application.Services.Saas;

namespace BarberSync.Tests;

public class SaasModuleTests
{
    [Fact]
    public void TenantService_Should_Return_Default_Plans()
    {
        var service = new SaasService(new InMemorySaasStore());
        Assert.True(service.GetPlans().Count >= 4);
    }

    [Fact]
    public void OnboardingService_Should_Progress_Steps()
    {
        var store = new InMemorySaasStore();
        var saas = new SaasService(store);
        var onboarding = new OnboardingService(store, saas);
        var start = onboarding.Start();
        var done = onboarding.Finish(start.TenantId);
        Assert.Equal("DONE", done.Step);
    }
}
