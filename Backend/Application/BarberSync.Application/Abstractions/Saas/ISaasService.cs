using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Saas;

public interface ISaasService
{
    IReadOnlyList<SubscriptionPlanDto> GetPlans();
    IReadOnlyList<SubscriptionDto> GetSubscriptions(Guid? tenantId);
    TenantUsageDto GetUsage(Guid tenantId);
    IReadOnlyList<InvoiceDto> GetInvoices(Guid tenantId);
    CompanyDto UpsertCompany(CompanyDto company);
    SubscriptionDto UpsertSubscription(SubscriptionDto subscription);
}
