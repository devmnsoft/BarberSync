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
    public Dictionary<Guid, TenantBrandingDto> TenantBranding { get; } = new();
    public Dictionary<Guid, TenantDomainDto> TenantDomains { get; } = new();
    public Dictionary<Guid, TenantPublicPageDto> TenantPublicPages { get; } = new();
    public List<PublicServiceDto> PublicServices { get; } = [];
    public List<PublicProfessionalDto> PublicProfessionals { get; } = [];
    public List<PublicAppointmentDto> PublicAppointments { get; } = [];
    public List<MarketplaceProfileDto> MarketplaceProfiles { get; } = [];
    public List<MarketplacePromotionDto> MarketplacePromotions { get; } = [];
    public List<BillingCycleDto> BillingCycles { get; } = [];
    public List<SubscriptionChangeDto> SubscriptionChanges { get; } = [];
    public List<SupportTicketDto> SupportTickets { get; } = [];
    public List<ProductEventDto> ProductEvents { get; } = [];
    public List<ExportJobDto> ExportJobs { get; } = [];
}
