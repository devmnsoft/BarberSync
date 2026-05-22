using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class SaasService(InMemorySaasStore store) : ISaasService
{
    public IReadOnlyList<SubscriptionPlanDto> GetPlans() => store.Plans;
    public IReadOnlyList<SubscriptionDto> GetSubscriptions(Guid? tenantId) => tenantId is null ? store.Subscriptions : store.Subscriptions.Where(x => x.TenantId == tenantId).ToList();
    public TenantUsageDto GetUsage(Guid tenantId) => store.Usage.TryGetValue(tenantId, out var usage) ? usage : new TenantUsageDto(tenantId, 0, 0, 0, 0, 0, 0);
    public IReadOnlyList<InvoiceDto> GetInvoices(Guid tenantId) => store.Invoices.Where(x => x.TenantId == tenantId).ToList();
    public CompanyDto UpsertCompany(CompanyDto company)
    {
        store.Companies.RemoveAll(x => x.Id == company.Id);
        store.Companies.Add(company);
        return company;
    }
    public SubscriptionDto UpsertSubscription(SubscriptionDto subscription)
    {
        store.Subscriptions.RemoveAll(x => x.Id == subscription.Id);
        store.Subscriptions.Add(subscription);
        return subscription;
    }
}
