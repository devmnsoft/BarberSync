using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarberSync.Application.Abstractions;
using BarberSync.Api.Security;

namespace BarberSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/saas")]
public class SaasController(ISaasService saasService, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("plans"), RequirePermission("Subscription.Manage")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanDto>>> Plans(CancellationToken ct) => Ok(await saasService.GetPlansAsync(ct));

    [HttpGet("subscriptions"), RequirePermission("Subscription.Manage")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionDto>>> Subscriptions(CancellationToken ct) => Ok(await saasService.GetSubscriptionsAsync(currentUser.TenantId,ct));

    [HttpGet("usage"), RequirePermission("Subscription.Manage")]
    public async Task<ActionResult<TenantUsageDto>> Usage(CancellationToken ct) => Ok(await saasService.GetUsageAsync(currentUser.TenantId,ct));

    [HttpGet("invoices"), RequirePermission("Subscription.Manage")]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> Invoices(CancellationToken ct) => Ok(await saasService.GetInvoicesAsync(currentUser.TenantId,ct));
}
