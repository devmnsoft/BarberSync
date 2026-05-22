using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class OnboardingService(InMemorySaasStore store, ISaasService saasService) : IOnboardingService
{
    public OnboardingStateDto Start()
    {
        var tenantId = Guid.NewGuid();
        var state = new OnboardingStateDto(tenantId, "STARTED", null, null, null);
        store.Onboarding[tenantId] = state;
        return state;
    }

    public OnboardingStateDto SetCompany(Guid tenantId, CompanyDto company) => Update(tenantId, s => s with { Step = "COMPANY", Company = saasService.UpsertCompany(company with { TenantId = tenantId }) });
    public OnboardingStateDto SetAdmin(Guid tenantId, string name, string email) => Update(tenantId, s => s with { Step = "ADMIN" });
    public OnboardingStateDto SetPlan(Guid tenantId, Guid planId) => Update(tenantId, s => s with { Step = "PLAN", Subscription = saasService.UpsertSubscription(new SubscriptionDto(Guid.NewGuid(), tenantId, planId, "ACTIVE", DateTime.UtcNow, null)) });
    public OnboardingStateDto SetBranding(Guid tenantId, CompanyBrandingDto branding) => Update(tenantId, s => s with { Step = "BRANDING", Branding = branding with { TenantId = tenantId } });
    public OnboardingStateDto Finish(Guid tenantId) => Update(tenantId, s => s with { Step = "DONE" });

    private OnboardingStateDto Update(Guid tenantId, Func<OnboardingStateDto, OnboardingStateDto> update)
    {
        var state = store.Onboarding.TryGetValue(tenantId, out var found) ? found : new OnboardingStateDto(tenantId, "STARTED", null, null, null);
        var next = update(state);
        store.Onboarding[tenantId] = next;
        return next;
    }
}
