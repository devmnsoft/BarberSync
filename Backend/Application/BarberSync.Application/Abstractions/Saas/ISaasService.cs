using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Saas;

public interface ISaasService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> GetPlansAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<TenantUsageDto> GetUsageAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken);
}
