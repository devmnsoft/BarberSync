using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class InMemorySaasStore
{
    public List<SubscriptionPlanDto> Plans { get; } =
    [
        new(Guid.NewGuid(), "STARTER", "Starter", 5, 3, 1, 500, false, false, false, false, false, 99),
        new(Guid.NewGuid(), "PROFESSIONAL", "Professional", 20, 10, 3, 4000, true, true, false, true, false, 299),
        new(Guid.NewGuid(), "PREMIUM", "Premium", 50, 25, 10, 15000, true, true, true, true, true, 799),
        new(Guid.NewGuid(), "ENTERPRISE", "Enterprise", 999, 999, 999, 999999, true, true, true, true, true, 1999)
    ];
    public List<CompanyDto> Companies { get; } = [];
    public List<SubscriptionDto> Subscriptions { get; } = [];
    public List<InvoiceDto> Invoices { get; } = [];
    public Dictionary<Guid, TenantUsageDto> Usage { get; } = new();
    public Dictionary<Guid, OnboardingStateDto> Onboarding { get; } = new();
    public List<ReviewDto> Reviews { get; } = [];
    public List<NpsResponseDto> NpsResponses { get; } = [];
}
